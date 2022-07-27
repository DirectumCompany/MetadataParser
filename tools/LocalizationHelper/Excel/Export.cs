using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace LocalizationHelper.Excel
{
  public static class Export
  {
    public static void ToExcel(string fileName, params Page[] pages)
    {
      var pagesWithData = pages.Where(p => p.Data.Any()).ToList();
      var app = new Application();
      app.SheetsInNewWorkbook = pagesWithData.Count;
      var workbooks = app.Workbooks;
      var book = workbooks.Add();
      var sheets = book.Sheets;

      var sheetIndex = 0;
      foreach (var page in pagesWithData)
      {
        sheetIndex++;
        Worksheet sheet = sheets[sheetIndex];
        sheet.Name = page.Title;

        // Шапка.
        var headerIndex = 0;
        foreach (var header in page.ColumnHeaders)
        {
          headerIndex++;
          sheet.Cells[1, headerIndex].Value2 = header;
        }

        // Заполнение данными.
        var cellIndex = 2;
        foreach (var grouping in page.Data.GroupBy(l => l.Entity))
        {
          var a = sheet.Range["A" + cellIndex, "A" + (cellIndex + grouping.Count() - 1)];
          a.Merge();
          a.VerticalAlignment = Constants.xlCenter;
          a.Value2 = grouping.Key;
          foreach (var line in grouping)
          {
            a = sheet.Range["B" + cellIndex];
            a.Value2 = line.Resourse;
            a = sheet.Range["C" + cellIndex];
            a.Value2 = line.RussianText;
            a = sheet.Range["D" + cellIndex];
            a.Value2 = line.EnglishText;
            a = sheet.Range["E" + cellIndex];
            a.Value2 = line.UsedInCode;
            a.VerticalAlignment = Constants.xlTop;
            a.ColumnWidth = 80;
            cellIndex++;
          }
        }

        // Оформление шапки + автоподбор ширины колонок.
        var range = sheet.Range["A1", sheet.Cells[1, headerIndex]];
        range.Font.Bold = true;
        range.Interior.ColorIndex = 16;
        range.EntireColumn.AutoFit();
      }

      book.SaveAs(Filename: fileName);
      book.Close();
      workbooks.Close();
      app.Quit();

      Marshal.ReleaseComObject(book);
      Marshal.ReleaseComObject(workbooks);
      Marshal.ReleaseComObject(app);
    }
  }
}