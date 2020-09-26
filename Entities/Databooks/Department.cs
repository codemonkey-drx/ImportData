using System;
using System.Collections.Generic;
using NLog;
using System.Linq;
using Sungero.Domain.Client;
using Sungero.Domain.ClientLinqExpressions;

namespace ImportData
{
  class Department : Entity
  {
    public int PropertiesCount = 8;
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
        var name = this.Parameters[shift + 0].Trim();
        if (string.IsNullOrEmpty(name))
        {
          var message = string.Format("Не заполнено поле \"Наименование\".");
          exceptionList.Add(new Structures.ExceptionsStruct {ErrorType = Constants.ErrorTypes.Error, Message = message});
          logger.Error(message);
          return exceptionList;
        }
        var shortName = this.Parameters[shift + 1].Trim();
        var code = this.Parameters[shift + 2].Trim();
        var businessUnit = BusinessLogic.GetBusinessUnit(session, this.Parameters[shift + 3].Trim(), exceptionList, logger);
        var headOffice = BusinessLogic.GetDepartment(session, this.Parameters[shift + 4].Trim(), exceptionList, logger);
        var manager = BusinessLogic.GetEmployee(session, this.Parameters[shift + 5].Trim(), exceptionList, logger);
        var phone = this.Parameters[shift + 6].Trim();
        var note = this.Parameters[shift + 7].Trim();

        try
        {
          var departments = Enumerable.ToList(session.GetAll<Sungero.Company.IDepartment>().Where(x => x.Name == name));
          var department = (Enumerable.FirstOrDefault<Sungero.Company.IDepartment>(departments));
          if (department == null)
            department = session.Create<Sungero.Company.IDepartment>();

          department.Name = name;
          department.ShortName = shortName;
          department.Code = code;
          department.BusinessUnit = businessUnit;
          department.HeadOffice = headOffice;
          department.Manager = manager;
          department.Phone = phone;
          department.Note = note;
          department.Save();
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
