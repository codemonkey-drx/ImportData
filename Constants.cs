using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportData
{
  class Constants
  {
    public class RolesGuides
    {
      public static readonly Guid RoleContractResponsible = new Guid("25C48B40-6111-4283-A94E-7D50E68DECC1");
      public static readonly Guid RoleIncomingDocumentsResponsible = new Guid("63EBE616-8780-4CBB-9AF7-C16251B38A84");
      public static readonly Guid RoleOutgoingDocumentsResponsible = new Guid("372D8FDB-316E-4F3C-9F6D-C2C1292BBFAE");
    }

    public class ErrorTypes
    {
      public const string Error = "Error";
      public const string Warn = "Warn";
      public const string Debug = "Debug";
    }

    public class SheetNames
    {
      public const string BusinessUnits = "НашиОрганизации";
      public const string Departments = "Подразделения";
      public const string Employees = "Сотрудники";
      public const string Companies = "Контрагенты";
      public const string Persons = "Персоны";
      public const string Contracts = "Договоры";
      public const string SupAgreements = "Доп.Соглашения";
      public const string IncomingLetters = "ВходящиеПисьма";
      public const string OutgoingLetters = "ИсходящиеПисьма";
    }

    public class Actions
    {
      public const string ImportCompany = "ImportCompany";
      public const string ImportCompanies = "ImportCompanies";
      public const string ImportPersons = "ImportPersons";
      public const string ImportContracts = "ImportContracts";
      public const string ImportSupAgreements = "ImportSupAgreements";
      public const string ImportIncomingLetters = "ImportIncomingLetters";
      public const string ImportOutgoingLetters = "ImportOutgoingLetters";

      public static Dictionary<string, string> dictActions = new Dictionary<string, string>
      {
        {ImportCompany, ImportCompany},
        {ImportCompanies, ImportCompanies},
        {ImportPersons, ImportPersons},
        {ImportContracts, ImportContracts},
        {ImportSupAgreements, ImportSupAgreements},
        {ImportIncomingLetters, ImportIncomingLetters},
        {ImportOutgoingLetters, ImportOutgoingLetters}
      };
    }
  }
}
