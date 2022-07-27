using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;

namespace LocalizationHelper.Resx
{
  public class ResourcesCollection : List<ResxLine>
  {
    public FileInfo DefaultFileInfo { get; set; }

    public FileInfo RussianFileInfo { get; set; }

    public ResourcesCollection GetResources(string searchPattern)
    {
      return new ResourcesCollection(this.Where(r => r.HasResource(searchPattern)))
      { DefaultFileInfo = DefaultFileInfo, RussianFileInfo = RussianFileInfo };
    }

    public ResourcesCollection GetRussianMissFilledResources()
    {
      return new ResourcesCollection(this.Where(r => r.HasRussianCharactersInDefault))
      { DefaultFileInfo = DefaultFileInfo, RussianFileInfo = RussianFileInfo };
    }

    public ResourcesCollection GetEnglishInRussianResources()
    {
      return new ResourcesCollection(this.Where(r => r.HasEnglishCharactersInRussian))
      { DefaultFileInfo = DefaultFileInfo, RussianFileInfo = RussianFileInfo };
    }

    public ResourcesCollection GetIncorrectSpacing()
    {
      return new ResourcesCollection(this.Where(r => r.HasIncorrectSpacing()).Select(r => new ResxLine()
      {
        Code = r.Code,
        Default = $"\"{r.Default}\"",
        Russian = $"\"{r.Russian}\""
      }))
      { DefaultFileInfo = DefaultFileInfo, RussianFileInfo = RussianFileInfo };
    }

    public bool HasResource(string resource)
    {
      return this.GetResources(resource).Any();
    }

    public bool HasRussianMissFilled()
    {
      return this.GetRussianMissFilledResources().Any();
    }

    public bool HasEnglishInRussian()
    {
      return this.GetEnglishInRussianResources().Any();
    }

    public bool HasIncorrectSpacing()
    {
      return this.GetIncorrectSpacing().Any();
    }

    public ResxLine SingleOrDefault(string key)
    {
      return this.SingleOrDefault(d => d.Code == key);
    }

    public ResourcesCollection(IEnumerable<ResxLine> entries)
    {
      this.AddRange(entries);
    }

    public ResourcesCollection(string folder, string name) : this(folder, name, string.Empty) { }

    public ResourcesCollection(string folder, string name, string postfix)
    {
      var defaultFile = string.Format(@"{0}\{1}{2}.resx", folder, name, postfix);
      var russianFile = string.Format(@"{0}\{1}{2}.ru.resx", folder, name, postfix);

      this.DefaultFileInfo = new FileInfo(defaultFile);

      this.RussianFileInfo = new FileInfo(russianFile);

      if (this.DefaultFileInfo.Exists)
        using (var resx = new ResXResourceReader(defaultFile))
        {
          foreach (DictionaryEntry entry in resx)
          {
            try
            {
              this.Add(new ResxLine { Code = entry.Key.ToString(), Default = entry.Value.ToString() });
            }
            catch (Exception e)
            {
              Console.WriteLine($"Localization not found: Key = {entry.Key.ToString()}");
              this.Add(new ResxLine { Code = entry.Key.ToString(), Default = string.Empty });
            }
          }
        }
      if (this.RussianFileInfo.Exists)
        using (var resx = new ResXResourceReader(russianFile))
        {
          foreach (DictionaryEntry entry in resx)
          {
            var line = this.SingleOrDefault(entry.Key.ToString());
            if (line == null)
            {
              line = new ResxLine { Code = entry.Key.ToString() };
              this.Add(line);
            }
            line.Russian = entry.Value.ToString();
          }
        }
    }
  }
}