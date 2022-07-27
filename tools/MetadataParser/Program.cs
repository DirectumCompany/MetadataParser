using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizationHelper.Resx;
using LocalizationHelper.Sources;
using MetadataParser.Core;
using Sungero.Metadata;

namespace MetadataParser
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

      // Загрузить строки локализации.
      var resources = LoadSystemResources(option.SourceFolder);

      // Загрузить метаданные.
      var modules = LoadModules(option.SourceFolder);

      // Спарсить описание метаданных.
      var totalDescription = new List<string>();
      foreach (var module in modules.OrderBy(x => x.Name))
      {
        // Модуль.
        var moduleDescription = MetadataParser.GetModuleDescription(module);
        totalDescription.AddRange(moduleDescription);

        var childEntities = module.Children.Cast<ModuleItemMetadata>()
          .Where(x => x is EntityMetadata && !MetadataParser.IsCollectionProperty(x as EntityMetadata))
          .OrderBy(x => x.Name);
        foreach (EntityMetadata entity in childEntities)
        {
          // Сущность.
          Console.WriteLine(entity.Name);
          var entityDescription = MetadataParser.GetEntityDescription(entity, resources);
          totalDescription.AddRange(entityDescription);

          // Свойства сущности без свойств-коллекций.
          var propertiesDescription = MetadataParser.GetPropertiesDescription(entity, resources);
          totalDescription.AddRange(propertiesDescription);
          totalDescription.Add(string.Empty);

          // Свойства-коллекции. Не выводить информацию по свойствам-коллекциям, добавленным в предках.
          var collectionProperties = module.Children.Cast<ModuleItemMetadata>()
            .Where(x => x is EntityMetadata)
            .Select(x => x as EntityMetadata)
            .Where(x => MetadataParser.IsCollectionProperty(x) &&
                        Equals(x.ActualMainEntityMetadata, entity) &&
                        !(x.ActualMainCollectionPropertyMetadata.IsAncestorMetadata))
            .OrderBy(x => x.Name)
            .ToList();
          var collectionPropertiesDescription = MetadataParser.GetCollectionPropertiesDescription(collectionProperties, resources);
          totalDescription.AddRange(collectionPropertiesDescription);

          // Добавить структуры сущности.
          var entityStructuresDescription = MetadataParser.GetStructuresDescription(entity.PublicStructures, false);
          totalDescription.AddRange(entityStructuresDescription);

          // Добавить константы сущности.
          var entityConstantsDescription = MetadataParser.GetConstantsDescription(entity.PublicConstants, false);
          totalDescription.AddRange(entityConstantsDescription);

          // Добавить пустую строку между разделами с сущностями.
          totalDescription.Add(string.Empty);
        }

        // Добавить структуры модуля.
        var moduleStructuresDescription = MetadataParser.GetStructuresDescription(module.PublicStructures, true);
        totalDescription.AddRange(moduleStructuresDescription);

        // Добавить константы модуля.
        var moduleConstantsDescription = MetadataParser.GetConstantsDescription(module.PublicConstants, true);
        totalDescription.AddRange(moduleConstantsDescription);

        // Добавить пустую строку между разделами с модулями.
        totalDescription.Add(string.Empty);
      }

      var requieredFieldsReportRequest = ReportGenerator.BuildReportRequest(modules, resources);

      File.WriteAllLines(option.TargetPath, totalDescription);
      string requestPath = Path.Combine(Path.GetDirectoryName(option.TargetPath), "ReportRequest.sql");
      File.WriteAllText(requestPath, requieredFieldsReportRequest);
      Console.WriteLine("Complete");
    }

    /// <summary>
    /// Загрузить системные ресурсы.
    /// </summary>
    /// <param name="versionFolder">Папка с исходниками.</param>
    /// <returns>Строки локализации.</returns>
    public static IEnumerable<Tuple<ResourcesCollection, string>> LoadSystemResources(string versionFolder)
    {
      Resources.LoadResources(versionFolder);
      Sources.LoadResources(versionFolder);
      return Resources.EntityResources.Select(r => Tuple.Create(r.SystemResource, r.Metadata.Name));
    }

    /// <summary>
    /// Загрузить метаданные.
    /// </summary>
    /// <param name="versionFolder">Папка с исходниками.</param>
    /// <returns>Метаданные.</returns>
    public static IEnumerable<ModuleMetadata> LoadModules(string versionFolder)
    {
      var modules = MetadataLoader.GetModules(versionFolder, false).ToList();
      foreach (var module in modules)
        Sungero.Metadata.Services.MetadataService.Instance.ModuleList.Modules.Add(module);
      return modules;
    }
  }
}

