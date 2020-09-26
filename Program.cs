﻿using System;
using System.Linq;
using Sungero.Domain.Client;
using System.Security;
using Sungero.Domain.ClientLinqExpressions;
using System.Collections.Generic;
using NLog;
using Sungero.Domain.Shared;
using Sungero.Domain.Client.Deployment;
using Sungero.Metadata.Services;
using CommonLibrary.Dependencies;
using Sungero.Domain.ClientBase;
using Sungero.Presentation;
using NDesk.Options;
using System.IO;
using System.Reflection;

namespace ImportData
{
  class Program
  {
    public static NLog.Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Обновление модулей.
    /// </summary>
    public static void UpdateModules()
    {
      ClientDevelopmentUpdater.Instance.RefreshDevelopment();
      MetadataService.ConfigurationSettingsPaths = new Sungero.Domain.ClientConfigurationSettingsPaths(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
      ClientLazyAssembliesResolver.Instance.LinkToAssembliesFolder(ClientDevelopmentUpdater.Instance.CacheFolder);

      var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
      var developmentDirectory = ClientDevelopmentUpdater.Instance.CacheFolder;

      Dependency.RegisterType<IClientLinqExtensions, ClientLinqExtensionsService>();
      Dependency.RegisterType<IHyperlinkDisplayTextCache, HyperlinkDisplayTextCacheImplementer>();
      Dependency.RegisterType<IHyperlinkEntityCache, HyperlinkEntityCacheImplementer>();

      LoadModules(baseDirectory, "*Client.dll");
      LoadModules(baseDirectory, "*Shared.dll");
      LoadModules(baseDirectory, "*Base.dll");
      LoadModules(developmentDirectory, null);

      EntityFactory.ConfigureUnityContainer();
    }


    /// <summary>
    /// Загрузка модулей.
    /// </summary>
    /// <param name="folderPath">Папка, где находятся модули.</param>
    /// <param name="mask">Маска для поиска файлов.</param>
    private static void LoadModules(string folderPath, string mask)
    {
      if (string.IsNullOrWhiteSpace(mask))
        ModuleManager.Instance.LoadModules(folderPath);
      else
        ModuleManager.Instance.LoadModules(folderPath, mask);
    }

    /// <summary>
    /// Выполнение импорта в соответствии с требуемым действием.
    /// </summary>
    /// <param name="action">Действие.</param>
    /// <param name="xlsxPath">Входной файл.</param>
    /// <param name="extraParameters">Дополнительные параметры.</param>
    /// <param name="logger">Логировщик.</param>
    /// <returns>Соответствующий тип сущности.</returns>
    static void ProcessByAction(string action, string xlsxPath, Dictionary<string, string> extraParameters, NLog.Logger logger)
    {
      switch (action)
      {
        case "ImportCompany":
          logger.Info("Импорт сотрудников");
          logger.Info("-------------");
          EntityProcessor.Procces(typeof(Employee), xlsxPath, Constants.SheetNames.Employees, extraParameters, logger);
          logger.Info("Импорт НОР");
          logger.Info("-------------");
          EntityProcessor.Procces(typeof(BusinessUnit), xlsxPath, Constants.SheetNames.BusinessUnits, extraParameters, logger);
          logger.Info("Импорт подразделений");
          logger.Info("-------------");
          EntityProcessor.Procces(typeof(Department), xlsxPath, Constants.SheetNames.Departments, extraParameters, logger);
          break;
        case "ImportCompanies":
          EntityProcessor.Procces(typeof(Company), xlsxPath, Constants.SheetNames.Companies, extraParameters, logger);
          break;
        case "ImportPersons":
          EntityProcessor.Procces(typeof(Person), xlsxPath, Constants.SheetNames.Persons, extraParameters, logger);
          break;
        case "ImportContracts":
          EntityProcessor.Procces(typeof(Contract), xlsxPath, Constants.SheetNames.Contracts, extraParameters, logger);
          break;
        case "ImportSupAgreements":
          EntityProcessor.Procces(typeof(SupAgreement), xlsxPath, Constants.SheetNames.SupAgreements, extraParameters, logger);
          break;
        case "ImportIncomingLetters":
          EntityProcessor.Procces(typeof(IncomingLetter), xlsxPath, Constants.SheetNames.IncomingLetters, extraParameters, logger);
          break;
        case "ImportOutgoingLetters":
          EntityProcessor.Procces(typeof(OutgoingLetter), xlsxPath, Constants.SheetNames.OutgoingLetters, extraParameters, logger);
          break;
        default:
          break;
      }
    }

    static void Main(string[] args)
    {
      logger.Info("=========================== Process Start ===========================");
      var watch = System.Diagnostics.Stopwatch.StartNew();

      #region Обработка параметров.

      var login = string.Empty;
      var password = string.Empty;
      var xlsxPath = string.Empty;
      var action = string.Empty;
      var extraParameters = new Dictionary<string, string>();

      bool isHelp = false;

      var p = new OptionSet() {
        { "n|name=",  "Имя учетной записи DirectumRX.", v => login = v },
        { "p|password=",  "Пароль учетной записи DirectumRX.", v => password = v },
        { "a|action=",  "Действие.", v => action = v },
        { "f|file=",  "Файл с исходными данными.", v => xlsxPath = v },
        { "dr|doc_register_id=",  "Журнал регистрации.", v => extraParameters.Add("doc_register_id", v)},
        { "h|help", "Show this help", v => isHelp = (v != null) },
      };

      try
      {
        p.Parse(args);
      }
      catch (OptionException e)
      {
        Console.WriteLine("Invalid arguments: " + e.Message);
        p.WriteOptionDescriptions(Console.Out);
        return;
      }

      if (isHelp || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(action)
        || string.IsNullOrEmpty(xlsxPath))
      {
        p.WriteOptionDescriptions(Console.Out);
        return;
      }

      #endregion

      try
      {
        if (!Constants.Actions.dictActions.ContainsKey(action))
        {
          var message = $"Не найдено действие \"{action}\". Введите действие корректно.";
          throw new Exception(message);
        }

        try
        {
          #region Аутентификация.
          System.Security.SecureString sec = new SecureString();
          var passwordCharArray = password.ToCharArray().ToList();
          foreach (var ch in passwordCharArray)
            sec.AppendChar(ch);
          sec.MakeReadOnly();
          var credentials = new UserCredentials(AuthenticationType.UserName, login, sec);
          UserCredentialsManager.Register(credentials, false);
          #endregion

          #region Обновление сборок.
          UpdateModules();
          #endregion

          #region Выполнение импорта сущностей.
          ProcessByAction(action, xlsxPath, extraParameters, logger);
          #endregion
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          logger.Error(ex.Message);
        }
        finally
        {
          UserCredentialsManager.Unregister();
        }
      }

      catch (Exception ex)
      {
        logger.Error(ex.Message);
      }

      finally
      {
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        logger.Info($"\nВремени затрачено: {elapsedMs} мс");
        logger.Info("=========================== Process Stop ===========================");
      }
    }
  }
}
