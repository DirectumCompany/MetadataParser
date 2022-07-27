using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationHelper.Excel;
using LocalizationHelper.Resx;

namespace LocalizationHelper
{
  class Program
  {
    static int Main(string[] args)
    {
      if (args.Length == 0 || !Directory.Exists(args[0]))
      {
        Console.WriteLine("Передайте папку с сорцами.");
        return -1;
      }

      Console.WriteLine("Загрузка ресурсов...");
      Resources.LoadResources(args[0]);
      Console.WriteLine("Загрузка сорцов...");
      Sources.Sources.LoadResources(args[0]);

      Console.WriteLine("Анализ...");
      var unusedLines = Line.EntitiesToLines(Resources.GetUnused(), false);

      var todo = "TODO";
      var todos = Resources.EntityResources.Where(r => r.HasResource(todo)).Select(r => r.GetResources(todo)).ToList();
      var russianInDefault = Resources.EntityResources.Where(r => r.HasRussianMissFilled()).Select(r => r.GetRussianMissFilled()).ToList();
      var englishInRussian = Resources.EntityResources.Where(r => r.HasEnglishInRussian()).Select(r => r.GetEnglishInRussian()).ToList();
      var spacing = Resources.EntityResources.Where(r => r.HasIncorrectSpacing()).Select(r => r.GetIncorrectSpacing()).ToList();

      var notLocalizedData = Line.EntitiesToLines(todos, true);
      // Выкидываем неиспользуемые ресурсы.
      notLocalizedData = notLocalizedData.Except(unusedLines).ToList();
      notLocalizedData.AddRange(Line.EntitiesToLines(russianInDefault, true));

      var todoPage = new Page
      {
        Title = "Не локализовано",
        Data = notLocalizedData,
        ColumnHeaders = new List<string>()
        {
          "Сущность",
          "Имя ресурса",
          "Русский текст ресурса",
          "Английский текст ресурса",
          "Использование",
          "Лист со скриншотом",
          "Исправленные русские строки",
          "Исправленное имя ресурса"
        }
      };
      var unused = new Page
      {
        Title = "Не используется",
        Data = unusedLines,
        ColumnHeaders = new List<string>()
        {
          "Сущность",
          "Имя ресурса",
          "Русский текст ресурса",
          "Английский текст ресурса",
        }
      };
      var uncorrectRussian = new Page
      {
        Title = "Англ символы в русском",
        Data = Line.EntitiesToLines(englishInRussian, false),
        ColumnHeaders = new List<string>()
        {
          "Сущность",
          "Имя ресурса",
          "Русский текст ресурса",
          "Английский текст ресурса",
        }
      };

      // Отображаем - разное число пробелов в начале и в конце строки для разных культур,
      // разное число переносов, наличие двойных пробелов.
      var spacingPage = new Page()
      {
        Title = "Несоответствие пробелов",
        Data = Line.EntitiesToLines(spacing, false),
        ColumnHeaders = new List<string>()
        {
          "Сущность",
          "Имя ресурса",
          "Русский текст ресурса",
          "Английский текст ресурса",
        }
      };

      Console.WriteLine("Экспорт...");
      var filePath = GetUniqueFilename("DirectumRX 2.3. Строки локализации. Спринт {0}.xlsx");
      try
      {
        Export.ToExcel(filePath, unused, todoPage, uncorrectRussian, spacingPage);
        Console.WriteLine("Завершено. Сохранено в {0}", filePath);
      }
      catch (Exception e)
      {
        Console.WriteLine("Возникли проблемы при экспорте в excel");
        Console.WriteLine(e);
      }
      return notLocalizedData.Count;
    }

    private static string GetUniqueFilename(string name)
    {
      int index = 1;
      var folder = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();
      var file = Path.Combine(folder, string.Format(name, index));
      while (File.Exists(file))
      {
        file = Path.Combine(folder, string.Format(name, index++));
      }
      return file;
    }
  }
}
