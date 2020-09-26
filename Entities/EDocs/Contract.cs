﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using System.Linq;
using Sungero.Domain.Client;
using Sungero.Domain.ClientLinqExpressions;

namespace ImportData
{
  public class Contract : Entity
  {
    public int PropertiesCount = 16;
    /// <summary>
    /// Получить наименование число запрашиваемых параметров.
    /// </summary>
    /// <returns>Число запрашиваемых параметров.</returns>
    public override int GetPropertiesCount()
    {
      return PropertiesCount;
    }

    /// <summary>
    /// Сохранение сущности в RX.
    /// </summary>
    /// <param name="shift">Сдвиг по горизонтали в XLSX документе. Необходим для обработки документов, составленных из элементов разных сущностей.</param>
    /// <param name="logger">Логировщик.</param>
    /// <returns>Число запрашиваемых параметров.</returns>
    public override IEnumerable<Structures.ExceptionsStruct> SaveToRX(NLog.Logger logger, int shift = 0)
    {
      var exceptionList = new List<Structures.ExceptionsStruct>();

      using (var session = new Session())
      {
        var regNumber = this.Parameters[shift + 0];
        DateTime? regDate = DateTime.MinValue;
        var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        var culture = CultureInfo.CreateSpecificCulture("en-GB");
        var regDateDouble = 0.0;
        if (!string.IsNullOrWhiteSpace(this.Parameters[shift + 1]) && !double.TryParse(this.Parameters[shift + 1].Trim(), style, culture, out regDateDouble))
        {
          var message = string.Format("Не удалось обработать дату регистрации \"{0}\".", this.Parameters[shift + 1]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }
        else
        {
          if (!string.IsNullOrEmpty(this.Parameters[shift + 1].ToString()))
            regDate = DateTime.FromOADate(regDateDouble);
        }

        var counterparty = BusinessLogic.GetConterparty(session, this.Parameters[shift + 2], exceptionList, logger);
        if (counterparty == null)
        {
          var message = string.Format("Не найден контрагент \"{0}\".", this.Parameters[shift + 2]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }

        var documentKind = BusinessLogic.GetDocumentKind(session, this.Parameters[shift + 3], exceptionList, logger);
        if (documentKind == null)
        {
          var message = string.Format("Не найден вид документа \"{0}\".", this.Parameters[shift + 3]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }

        var contractCategory = BusinessLogic.GetContractCategory(session, this.Parameters[shift + 4], exceptionList, logger);
        if (!string.IsNullOrEmpty(this.Parameters[shift + 4].ToString()))
        {
          if (contractCategory == null)
          {
            var message = string.Format("Не найдена категория договора \"{0}\".", this.Parameters[shift + 4]);
            exceptionList.Add(new Structures.ExceptionsStruct{ErrorType = Constants.ErrorTypes.Error, Message = message});
            logger.Error(message);
            return exceptionList;
          }
        }

        var subject = this.Parameters[shift + 5];

        var department = BusinessLogic.GetDepartment(session, this.Parameters[shift + 6], exceptionList, logger);
        if (department == null)
        {
          var message = string.Format("Не найдено подразделение \"{0}\".", this.Parameters[shift + 6]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }

        var filePath = this.Parameters[shift + 7];

        var validFrom = new DateTime();
        var validFromDouble = 0.0;
        if (!string.IsNullOrWhiteSpace(this.Parameters[shift + 8]) && !double.TryParse(this.Parameters[shift + 8].Trim(), style, culture, out validFromDouble))
        {
          var message = string.Format("Не удалось обработать значение в поле \"Действует с\" \"{0}\".", this.Parameters[shift + 8]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }
        else
        {
          if (!string.IsNullOrEmpty(this.Parameters[shift + 8].ToString()))
            validFrom = DateTime.FromOADate(validFromDouble);
        }

        var validTill = new DateTime();
        var validTillDouble = 0.0;
        if (!string.IsNullOrWhiteSpace(this.Parameters[shift + 9]) && !double.TryParse(this.Parameters[shift + 9].Trim(), style, culture, out validTillDouble))
        {
          var message = string.Format("Не удалось обработать значение в поле \"Действует по\" \"{0}\".", this.Parameters[shift + 9]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }
        else
        {
          if (!string.IsNullOrEmpty(this.Parameters[shift + 9].ToString()))
            validTill = DateTime.FromOADate(validTillDouble);
        }

        var totalAmount = 0.0;
        if (!string.IsNullOrWhiteSpace(this.Parameters[shift + 10]) && !double.TryParse(this.Parameters[shift + 10].Trim(), style, culture, out totalAmount))
        {
          var message = string.Format("Не удалось обработать значение в поле \"Сумма\" \"{0}\".", this.Parameters[shift + 10]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }

        var currency = BusinessLogic.GetCurrency(session, this.Parameters[shift + 11], exceptionList, logger);
        if (currency == null)
        {
          var message = string.Format("Не найдено соответствующее наименование валюты \"{0}\".", this.Parameters[shift + 11]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }
        var lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(session, this.Parameters[shift + 12]);
        if (lifeCycleState == null)
        {
          var message = string.Format("Не найдено соответствующее значение состояния \"{0}\".", this.Parameters[shift + 12]);
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }
        var responsibleEmployee = BusinessLogic.GetEmployee(session, this.Parameters[shift + 13], exceptionList, logger);
        var ourSignatory = BusinessLogic.GetEmployee(session, this.Parameters[shift + 14], exceptionList, logger);
        var note = this.Parameters[shift + 15];
        try
        {
          var contracts = Enumerable.ToList(session.GetAll<Sungero.Contracts.IContract>().Where(x => Equals(x.RegistrationNumber, regNumber) &&
                                                                                                Equals(x.RegistrationDate, regDate) &&
                                                                                                Equals(x.Counterparty, counterparty)));
          var contract = (Enumerable.FirstOrDefault<Sungero.Contracts.IContract>(contracts));
          if (contract != null)
          {
            var message = string.Format("Договор не может быть импортирован. Найден дубль с такими же реквизитами \"Рег. №\" {0}, \"Дата документа\" {1}, \"Контрагент\" {2}.", regNumber, regDate.ToString(), counterparty.ToString());
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);
            return exceptionList;
          }
          contract = session.Create<Sungero.Contracts.IContract>();
          contract.Counterparty = counterparty;
          contract.DocumentKind = documentKind;
          contract.DocumentGroup = contractCategory;
          contract.Subject = subject;
          contract.Department = department;
          contract.ValidFrom = validFrom;
          contract.ValidTill = validTill;
          contract.TotalAmount = totalAmount;
          contract.Currency = currency;
          contract.LifeCycleState = lifeCycleState;
          contract.ResponsibleEmployee = responsibleEmployee;
          contract.OurSignatory = ourSignatory;
          contract.Note = note;
          if (regDate != DateTime.MinValue)
            contract.RegistrationDate = regDate;
          contract.RegistrationNumber = regNumber;
          contract.Save();
          if (!string.IsNullOrWhiteSpace(filePath))
            exceptionList.Add(BusinessLogic.ImportBody(session, contract, filePath, logger));
          var documentRegisterId = 0;
          if (ExtraParameters.ContainsKey("doc_register_id"))
            if (int.TryParse(ExtraParameters["doc_register_id"], out documentRegisterId))
              exceptionList.AddRange(BusinessLogic.RegisterDocument(session, contract, documentRegisterId, regNumber, regDate, Constants.RolesGuides.RoleContractResponsible, logger));
            else
            {
              var message = string.Format("Не удалось обработать параметр \"doc_register_id\". Полученное значение: {0}.", ExtraParameters["doc_register_id"]);
              exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
              logger.Error(message);
              return exceptionList;
            }
        }
        catch (Exception ex)
        {
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = ex.Message});
          return exceptionList;
        }
        session.SubmitChanges();
      }
      return exceptionList;
    }
  }
}
