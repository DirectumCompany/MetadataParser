using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataVersionComparer.Core;
using Sungero.Metadata;
using MetadataParser.Core;

namespace MetadataVersionComparer
{
  class Program
  {
    static void Main(string[] args)
    {
      var option = new Option();
      if (!CommandLine.Parser.Default.ParseArguments(args, option))
      {
        Console.WriteLine("Args '{0}' not parsed.", string.Join(", ", args));
        Console.WriteLine(option.GetUsage());
        Environment.Exit(-1);
      }

      var oldModules = MetadataLoader.GetModules(option.OldVersionFolder, false).ToList();
      var newModules = MetadataLoader.GetModules(option.NewVersionFolder, false).ToList();

      // Те метаданные, что новее, грузим для работы ссылок между сущностями.
      foreach (var module in newModules)
      {
        Sungero.Metadata.Services.MetadataService.Instance.ModuleList.Modules.Add(module);
      }

      var allDiffs = new List<Diff>();
      foreach (var newModule in newModules)
      {
        var oldModule = oldModules.GetThisChild(newModule);
        var diffsModule = newModule.Compare(oldModule);
        foreach (var diff in diffsModule)
        {
          diff.Module = newModule.Name;
          allDiffs.Add(diff);
        }

        var childs = newModule.Children.Cast<ModuleItemMetadata>().ToList();
        foreach (var child in childs)
        {
          var oldItem = oldModules.GetThisChild(child);
          var diffsChild = child.Compare(oldItem);
          foreach (var diff in diffsChild)
          {
            diff.Module = newModule.Name;
            allDiffs.Add(diff);
          }
        }
      }

      var file = option.DiffPath;
      var text = new List<string>();
      foreach (var moduleDiffs in allDiffs.GroupBy(d => d.Module).OrderBy(e => e.Key))
      {
        // Модуль.
        text.Add($"# {moduleDiffs.Key}\n");

        foreach (var entityDiffs in moduleDiffs.GroupBy(d => d.Entity).OrderBy(e => e.Key))
        {
          // Сущность.
          text.Add($"## {entityDiffs.Key}\n");

          text.AddRange(entityDiffs.OfType<EntityDiff>().Where(e => !e.IsChild).Select(d => d.ToString()));
          text.AddRange(entityDiffs.OfType<PropertyDiff>().Where(e => !e.IsChild).Select(d => d.ToString()));
          foreach (var childGroup in entityDiffs.Where(e => e.IsChild).GroupBy(e => e.Child))
          {
            // Добавить пустую строку между разделами.
            if (entityDiffs.OfType<PropertyDiff>().Where(e => !e.IsChild).Any() ||
              entityDiffs.OfType<EntityDiff>().Where(e => !e.IsChild).Any())
              text.Add(string.Empty);

            text.Add("### Свойство-коллекция " + childGroup.Key + ":\n");
            text.AddRange(childGroup.OfType<EntityDiff>().Select(d => d.ToString()));
            text.AddRange(childGroup.OfType<PropertyDiff>().Select(d => d.ToString()));
          }

          // Добавить пустую строку между разделами.
          text.Add(string.Empty);
        }
      }
      File.WriteAllLines(file, text);
    }
  }
}
