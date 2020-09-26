using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ImportData
{
  public class EntityProcessor
  {
    public static void Procces(Type type, string xlsxPath, string sheetName, Dictionary<string, string> extraParameters, Logger logger)
    {
      if (type.Equals(typeof(Entity)))
      {
        logger.Error(string.Format("Не найден соответствующий обработчик операции: {0}", "action"));
        return;
      }
      Type genericType = typeof(EntityWrapper<>);
      Type[] typeArgs = { Type.GetType(type.ToString()) };
      Type wrapperType = genericType.MakeGenericType(typeArgs);
      object processor = Activator.CreateInstance(wrapperType);
      var getEntity = wrapperType.GetMethod("GetEntity");

      uint row = 2;
      var excelProcessor = new ExcelProcessor(xlsxPath, sheetName, logger);
      var importData = excelProcessor.GetDataFromExcel();
      var parametersListCount = importData.Count() - 1;
      var importItemCount = importData.First().Count();
      var exceptionList = new List<Structures.ExceptionsStruct>();
      var arrayItems = new ArrayList();
      var listImportItems = new List<string[]>();
      int paramCount = 0;
      var listResult = new List<List<Structures.ExceptionsStruct>>();
      Console.WriteLine("===================Чтение строк из файла===================");
      // Пропускаем 1 строку, т.к. в ней заголовки таблицы.
      foreach (var importItem in importData.Skip(1))
      {
        var percent = (double)(row - 1) / (double)parametersListCount * 100.00;
        Console.WriteLine($"\rОбработано {row - 1} строк из {parametersListCount} ({percent:F2}%)");

        int countItem = importItem.Count();
        foreach (var data in importItem.Take(countItem - 3))
          arrayItems.Add(data);

        listImportItems.Add((string[])arrayItems.ToArray(typeof(string)));
        arrayItems.Clear();
        row++;
      }
      Console.WriteLine("======================Импорт сущностей=====================");
      row = 2;
      foreach (var importItem in listImportItems)
      {
        var entity = (Entity)getEntity.Invoke(processor, new object[] { importItem.ToArray(), extraParameters });
        if (entity != null)
        {
          if (importItemCount >= entity.GetPropertiesCount())
          {
            exceptionList = entity.SaveToRX(logger).ToList();
            if (!exceptionList.Any())
            {
              var percent = (double)(row - 1) / (double)parametersListCount * 100.00;
              Console.WriteLine($"\rИмпортировано {row - 1} сущностей из {parametersListCount} ({percent:F2}%)");
              row++;
            }
          }
          else
          {
            var message = string.Format("Количества входных параметров недостаточно. " +
              "Количество ожидаемых параметров {0}. Количество переданных параметров {1}.", entity.GetPropertiesCount(), importItemCount);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);
          }
          listResult.Add(exceptionList);
        }
        if (paramCount == 0)
          paramCount = entity.GetPropertiesCount();
      }

      Console.WriteLine("=============Запись результатов импорта в файл=============");
      row = 2;

      var listArrayParams = new List<ArrayList>();
      string[] text = new string[] { "Итог", "Дата", "Подробности" };
      for (int i = 1; i <= 3; i++)
      {
        var title = excelProcessor.GetExcelColumnName(paramCount + i);
        var arrayParams = new ArrayList { text[i - 1], title, 1 };
        listArrayParams.Add(arrayParams);
      }

      foreach (var result in listResult)
      {
        if (result.Where(x => x.ErrorType == Constants.ErrorTypes.Error).Any())
        {
          // TODO: Добавить локализацию строки.
          var message = string.Join("; ", result.Where(x => x.ErrorType == Constants.ErrorTypes.Error).Select(x => x.Message).ToArray());
          text = null;
          text = new string[] { "Не загружен", DateTime.Now.ToString("d"), message };
          for (int i = 1; i <= 3; i++)
          {
            var title = excelProcessor.GetExcelColumnName(paramCount + i);
            var arrayParams = new ArrayList { text[i - 1], title, row };
            listArrayParams.Add(arrayParams);
          }
        }
        else if (result.Where(x => x.ErrorType == Constants.ErrorTypes.Warn).Any())
        {
          // TODO: Добавить локализацию строки.
          var message = string.Join(Environment.NewLine, result.Where(x => x.ErrorType == Constants.ErrorTypes.Warn).Select(x => x.Message).ToArray());
          text = null;
          text = new string[] { "Загружен частично", DateTime.Now.ToString("d"), message };
          for (int i = 1; i <= 3; i++)
          {
            var title = excelProcessor.GetExcelColumnName(paramCount + i);
            var arrayParams = new ArrayList { text[i - 1], title, row };
            listArrayParams.Add(arrayParams);
          }
        }
        else
        {
          // TODO: Добавить локализацию строки.
          text = null;
          text = new string[] { "Загружен", DateTime.Now.ToString("d"), string.Empty };
          for (int i = 1; i <= 3; i++)
          {
            var title = excelProcessor.GetExcelColumnName(paramCount + i);
            var arrayParams = new ArrayList { text[i - 1], title, row };
            listArrayParams.Add(arrayParams);
          }
        }
        row++;
      }
      excelProcessor.InsertText(listArrayParams);
    }
  }
}
