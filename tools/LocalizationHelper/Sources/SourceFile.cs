using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LocalizationHelper.Resx;

namespace LocalizationHelper.Sources
{
  public class SourceFile
  {
    public FileInfo FileInfo { get; set; }

    public string Content { get; set; }

    public SourceFile(string file)
    {
      this.FileInfo = new FileInfo(file);

      if (this.FileInfo.Exists)
        this.Content = File.ReadAllText(this.FileInfo.FullName);
    }

    public string GetResources(ResxLine res)
    {
      var result = new List<string>();
      var pattern = string.Format(@"Converter\(\""{0}\""\)|Resources\s*\.\s*{0}|Resources\s*\.\s*\w+Report\s*\.\s*{0}", res.Code);
      var match = Regex.Match(this.Content, pattern, RegexOptions.IgnoreCase);
      if (match.Success)
      {
        var list = this.Content.Split('\r', '\n').ToList();
        var index = list.FindIndex(l => l.Contains(res.Code));
        result.Add(string.Format("{0}\\{1} : {2}", this.FileInfo.Directory.Name, this.FileInfo.Name, index+1));
        result.Add(string.Join(Environment.NewLine, list.GetRange(Math.Max(index - 2, 0), 5)).Trim());
      }

      return string.Join(Environment.NewLine, result).Trim();
    }

    public bool HasResource(ResxLine res)
    {
      return this.GetResources(res).Any();
    }
  }
}