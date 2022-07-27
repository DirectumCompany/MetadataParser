using System.Collections.Generic;
using System.Linq;
using LocalizationHelper.Resx;

namespace LocalizationHelper.Excel
{
  public class Line
  {
    public string Entity { get; set; }

    public string Resourse { get; set; }

    public string RussianText { get; set; }

    public string EnglishText { get; set; }

    public string UsedInCode { get; set; }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != this.GetType())
        return false;

      return Equals((Line) obj);
    }

    protected bool Equals(Line other)
    {
      return string.Equals(Entity, other.Entity) && string.Equals(Resourse, other.Resourse);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (Entity.GetHashCode() * 397) ^ Resourse.GetHashCode();
      }
    }

    public static bool operator ==(Line left, Line right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Line left, Line right)
    {
      return !Equals(left, right);
    }

    public override string ToString()
    {
      return string.Format("{0}->{1}:{2}|{3}", Entity, Resourse, RussianText, EnglishText);
    }

    public static List<Line> EntitiesToLines(List<Entity> entities, bool needSourceCode)
    {
      var lines = new List<Line>();
      foreach (var entry in entities)
      {
        foreach (var str in entry.Resources)
        {
          var isSystem = entry.SystemResource == str;
          var names = str.Select(s => s.Code).ToList();
          foreach (var name in names)
          {
            var resx = str.SingleOrDefault(name);
            var sourceFile = needSourceCode && !isSystem ? Sources.Sources.SourceFiles.FirstOrDefault(s => s.HasResource(resx)) : null;
            var line = new Line
            {
              Entity = isSystem ? string.Format("{0} (System)", entry) : entry.ToString(),
              Resourse = name,
              EnglishText = resx.Default, 
              RussianText = resx.Russian,
              UsedInCode = sourceFile != null ? sourceFile.GetResources(resx) : string.Empty
            };
            lines.Add(line);
          }
        }
      }
      return lines;
    }
  }
}