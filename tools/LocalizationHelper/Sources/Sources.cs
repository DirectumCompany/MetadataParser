using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalizationHelper.Sources
{
  public static class Sources
  {
    public static List<SourceFile> SourceFiles { get; set; }
    
    public static List<SourceFile> ReportFiles { get; set; }

    public static void LoadResources(string path)
    {
      var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories).Where(f => !f.EndsWith(".g.cs"));
      SourceFiles.AddRange(files.Select(f => new SourceFile(f)));

      var reports = Directory.GetFiles(path, "*.frx", SearchOption.AllDirectories);
      ReportFiles.AddRange(reports.Select(f => new SourceFile(f)));
    }

    static Sources()
    {
      SourceFiles = new List<SourceFile>();
      ReportFiles = new List<SourceFile>();
    }
  }
}