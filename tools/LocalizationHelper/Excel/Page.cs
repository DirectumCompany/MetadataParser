using System.Collections.Generic;

namespace LocalizationHelper.Excel
{
  public class Page
  {
    public string Title { get; set; }

    public List<string> ColumnHeaders { get; set; }

    public List<Line> Data { get; set; }

    public Page()
    {
      this.ColumnHeaders = new List<string>();
      this.Data = new List<Line>();
    }
  }
}