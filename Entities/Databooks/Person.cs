﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using System.Linq;
using Sungero.Domain.Client;
using Sungero.Domain.ClientLinqExpressions;

namespace ImportData
{
  class Person : Entity
  {
    public int PropertiesCount = 17;
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
        var lastName = this.Parameters[shift + 0].Trim();
        if (string.IsNullOrEmpty(lastName))
        {
          var message = string.Format("Не заполнено поле \"Фамилия\".");
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = "Error", Message = message});
          logger.Error(message);
          return exceptionList;
        }
        var firstName = this.Parameters[shift + 1].Trim();
        if (string.IsNullOrEmpty(firstName))
        {
          var message = string.Format("Не заполнено поле \"Имя\".");
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = "Error", Message = message});
          logger.Error(message);
          return exceptionList;
        }
        var middleName = this.Parameters[shift + 2].Trim();
        var sex = BusinessLogic.GetPropertySex(session, this.Parameters[shift + 3].Trim());
        var dateOfBirth = DateTime.MinValue;
        var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        var culture = CultureInfo.CreateSpecificCulture("en-GB");
        var dateOfBirthDouble = 0.0;
        if (!string.IsNullOrWhiteSpace(this.Parameters[shift + 4]))
        {
          if (double.TryParse(this.Parameters[shift + 4].Trim(), style, culture, out dateOfBirthDouble))
            dateOfBirth = DateTime.FromOADate(dateOfBirthDouble);
          else
          {
            var message = string.Format("Не удалось обработать значение в поле \"Дата рождения\" \"{0}\".", this.Parameters[shift + 4]);
            exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Warn, Message = message});
            logger.Warn(message);
          }
        }
        var tin = this.Parameters[shift + 5].Trim();
        var inila = this.Parameters[shift + 6].Trim();
        var city = BusinessLogic.GetCity(session, this.Parameters[shift + 7].Trim(), exceptionList, logger);
        var region = BusinessLogic.GetRegion(session, this.Parameters[shift + 8].Trim(), exceptionList, logger);
        var legalAdress = this.Parameters[shift + 9].Trim();
        var postalAdress = this.Parameters[shift + 10].Trim();
        var phones = this.Parameters[shift + 11].Trim();
        var email = this.Parameters[shift + 12].Trim();
        var homepage = this.Parameters[shift + 13].Trim();
        var bank = BusinessLogic.GetBank(session, this.Parameters[shift + 14], exceptionList, logger);
        if (!string.IsNullOrEmpty(this.Parameters[shift + 14]))
        {
          if (bank == null)
          {
            var message = $"Не найден банк \"{this.Parameters[shift + 14]}\".";
            exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
            logger.Error(message);
            return exceptionList;
          }
        }
        var account = this.Parameters[shift + 15].Trim();
        var note = this.Parameters[shift + 16].Trim();

        try
        {
          var persons = Enumerable.ToList(session.GetAll<Sungero.Parties.IPerson>().Where(x => x.LastName == lastName && x.FirstName == firstName));
          var person = (Enumerable.FirstOrDefault<Sungero.Parties.IPerson>(persons));
          if (person != null)
          {
            var message = string.Format("Персона не может быть импортирована. Найден дубль по реквизитам Фамилия: \"{0}\", Имя: {1}.", lastName, firstName);
            exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = "Error", Message = message});
            logger.Error(message);
            return exceptionList;
          }
          person = session.Create<Sungero.Parties.IPerson>();

          person.LastName = lastName;
          person.FirstName = firstName;
          person.MiddleName = middleName;
          person.Sex = sex;
          if (dateOfBirth != DateTime.MinValue)
            person.DateOfBirth = dateOfBirth;
          person.TIN = tin;
          person.INILA = inila;
          if (city != null)
            person.City = city;
          if (region != null)
            person.Region = region;
          person.LegalAddress = legalAdress;
          person.PostalAddress = postalAdress;
          person.Phones = phones;
          person.Email = email;
          person.Homepage = homepage;
          person.Bank = bank;
          person.Account = account;
          person.Note = note;
          person.Save();

        }
        catch (Exception ex)
        {
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = "Error", Message = ex.Message});
          return exceptionList;
        }
        session.SubmitChanges();
      }
      return exceptionList;
    }
  }
}
