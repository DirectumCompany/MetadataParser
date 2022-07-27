using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LocalizationHelper.Resx;
using Sungero.Metadata;
using MetadataParser;

namespace MetadataParser.Core
{
  internal class ReportGenerator
  {
    #region Константы
    /// <summary>
    /// SQL-запрос для создания таблицы отчета по незаполненным полям.
    /// </summary>
    private const string ReportTableRequest =
@"DROP TABLE IF EXISTS RequiredFields;

CREATE TABLE RequiredFields(
ID int IDENTITY(1,1) PRIMARY KEY,
EntityName varchar(250),
EntityRuLocalization varchar(250),
EntityTable varchar(250),
EntityID varchar(250),
PropertyName varchar(250),
PropertyRuLocalization varchar(250),
PropertyIsCollection bit,
PropertyColumnName varchar(250),
PropertyTableName varchar(250),
CollectionPropertyName varchar(250),
CollectionPropertyRuLocalization varchar(250),
CollectionPropertyColumnName varchar(250));
";
    /// <summary>
    /// Шаблон SQL-запроса для проверки заполненности свойства типа сущности в базе данных системы.
    /// </summary>
    private const string PropertyAddRequest =
@"INSERT INTO RequiredFields(EntityName, EntityRuLocalization, EntityTable, EntityID, PropertyName, PropertyRuLocalization, PropertyColumnName, PropertyIsCollection) 
SELECT '{0}', '{1}', '{2}', Id, '{3}', '{4}', '{5}', {6}
FROM [{2}]
WHERE ([{5}] IS NULL OR [{5}] = '') AND Discriminator = '{7}';
";
    /// <summary>
    /// Шаблон SQL-запроса для проверки заполненности свойства-коллекции типа сущности в базе данных системы.
    /// </summary>
    private const string CollectionAddRequest =
@"INSERT INTO RequiredFields(EntityName, EntityRuLocalization, EntityTable, EntityID, PropertyName, PropertyRuLocalization, PropertyTableName, PropertyIsCollection) 
SELECT '{0}', '{1}', '{2}', a.Id, '{3}', '{4}', '{5}', {6}
FROM (SELECT * FROM [{2}]  WHERE Discriminator = '{7}' AND [{2}].[Id] not in 
(SELECT [{8}] FROM [{5}])) as a
";
    /// <summary>
    /// Шаблон SQL-запроса для проверки заполненности свойства свойства-коллекции в базе данных системы.
    /// </summary>
    private const string SubPropertyAddRequest =
@"INSERT INTO RequiredFields (EntityName, EntityRuLocalization, EntityTable, EntityID, PropertyName, PropertyRuLocalization,PropertyTableName, PropertyIsCollection,CollectionPropertyName, CollectionPropertyRuLocalization, CollectionPropertyColumnName) 
SELECT '{0}', '{1}', '{2}', Id, '{3}', '{4}', '{5}', {6}, '{7}', '{8}', '{9}'
FROM [{5}]
WHERE ([{9}] IS NULL OR [{9}] = '') AND Discriminator = '{10}';
";


    #endregion Константы

    /// <summary>
    /// Формирует SQl-скрипт, генерирующий таблицу-отчет с информацией о незаполненных обязательных свойствах типов документов и справочников системы. 
    /// </summary>
    /// <param name="modules">Метаданные системы.</param>
    /// <param name="resources">Строки локализации.</param>
    /// <returns>SQl-скрипт</returns>
    internal static string BuildReportRequest(IEnumerable<ModuleMetadata> modules, IEnumerable<Tuple<ResourcesCollection, string>> resources)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(ReportTableRequest);

      foreach (var module in modules.OrderBy(x => x.Name))
      {
        sb.Append($"\n-- #{module.Name}\n");

        // Только документы и справочники.
        var childEntities = module.Children.Cast<ModuleItemMetadata>()
          .Where(x => x is EntityMetadata && !MetadataParser.IsCollectionProperty(x as EntityMetadata))
          .Where(x => x is DocumentMetadata ||
          (x as EntityMetadata).AscendantEntities.Select(e => e.DBTableName).Contains("Sungero_Core_Databook"))
          .OrderBy(x => x.Name);
        foreach (EntityMetadata entity in childEntities)
        {
          sb.Append($"\n-- ##{entity.Name}\n");
          // Обязательные свойства типа сущности, кроме коллекций.
          var properties = entity?.Properties?.Where(x => !(x is CollectionPropertyMetadata) && x.IsRequired);
          if (properties != null && properties.Any())
            foreach (var property in properties)
              sb.Append(GetSQLRequestCheckingFullnessEntityProperty(property, resources));

          // Свойства-коллекции типа сущности.
          var collectionProperties = module.Children.Cast<ModuleItemMetadata>()
              .Where(x => x is EntityMetadata)
              .Select(x => x as EntityMetadata)
              .Where(x => MetadataParser.IsCollectionProperty(x) && Equals(x.ActualMainEntityMetadata, entity))
              .OrderBy(x => x.Name)
              .ToList();
          foreach (EntityMetadata collection in collectionProperties)
          {
            if (collection.ActualMainCollectionPropertyMetadata.IsRequired)
              sb.Append(GetSQLRequestCheckingFullnessEntityColliectionProperty(collection, resources));

            // Обязательные свойства в свойствах-коллекциях типа сущности.
            var subProperties = collection?.Properties?.Where(x => x.IsRequired);
            if (subProperties != null && subProperties.Any())
              foreach (var subProperty in subProperties)
                sb.Append(GetSQLRequestCheckingFullnessColliectionProperty(subProperty, resources));
          }
        }
      }
      sb.AppendLine("SELECT * FROM RequiredFields");
      return sb.ToString();
    }

    /// <summary>
    /// Получить свойство-предок.
    /// </summary>
    /// <param name="propertyHeir">Свойство-наследник.</param>
    /// <returns> Метаданные свойства-предка.</returns>
    private static PropertyMetadata GetAncestorProperty(PropertyMetadata propertyHeir)
    {
      var ancestorEntity = propertyHeir.ParentEntityMetadata;
      while (!ancestorEntity.Properties.Where(p => p.Name == propertyHeir.Name && !p.IsAncestorMetadata).Any())
        ancestorEntity = ancestorEntity.BaseEntityMetadata;
      var ancestorProperty = ancestorEntity.Properties.Where(p => p.Name == propertyHeir.Name && !p.IsAncestorMetadata).FirstOrDefault();
      return ancestorProperty;
    }

    /// <summary>
    /// Получить строку локализации (Русская приоритетная).
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="mtdName">Наименование файла с mtd, к которому привязаны ресурсы.</param>
    /// <param name="entityName">Наименование сущности или свойства-коллекции, в ресурсах которого производится поиск.</param>
    /// <returns>Строка локализации.</returns>
    private static string GetLocalizationString(IEnumerable<Tuple<ResourcesCollection, string>> resources,
      string mtdName, string entityName)
    {
      var resourceFiles = resources.Where(r => r.Item2 == $"{ mtdName }.mtd");

      foreach (var resource in resourceFiles)
      {
        var res = resource.Item1.Where(r => r.Code == entityName && !string.IsNullOrEmpty(r.Russian)).FirstOrDefault();
        if (res != null) return res.Russian;
      }

      foreach (var resource in resourceFiles)
      {
        var res = resource.Item1.Where(r => r.Code == entityName && !string.IsNullOrEmpty(r.Default)).FirstOrDefault();
        if (res != null) return res.Default;
      }

      return string.Empty;
    }

    /// <summary>
    /// Получить SQL-запрос, проверяющий заполненность свойства в базе данных системы.
    /// </summary>
    /// <param name="property">Метаданные свойства.</param>
    /// <param name="resources">Системные ресурсы.</param>
    /// <returns>SQL-запрос</returns>
    private static string GetSQLRequestCheckingFullnessEntityProperty(PropertyMetadata property, IEnumerable<Tuple<ResourcesCollection, string>> resources)
    {
      var entity = property.ParentEntityMetadata;
      var entityLocalization = ReportGenerator.GetLocalizationString(resources, entity.Name, "DisplayName") ??
        ReportGenerator.GetLocalizationString(resources, entity.Name, "AccusativeDisplayName");

      StringBuilder sb = new StringBuilder();
      sb.Append($"\n-- ###{property.Name}\n");
      if (property.IsAncestorMetadata)
      {
        var ancestorProperty = GetAncestorProperty(property);
        var propertyLocalization = ReportGenerator.GetLocalizationString(resources, ancestorProperty.ParentEntityMetadata.Name, $"Property_{ancestorProperty.Name}");
        sb.AppendFormat(PropertyAddRequest, entity.Name, entityLocalization, entity.DBTableName, property.Name, propertyLocalization,
          ancestorProperty.DBColumnName, 0, entity.NameGuid);
      }
      else
      {
        var propertyLocalization = ReportGenerator.GetLocalizationString(resources, entity.Name, $"Property_{property.Name}");
        sb.AppendFormat(PropertyAddRequest, entity.Name, entityLocalization, entity.DBTableName, property.Name, propertyLocalization,
          property.DBColumnName, 0, entity.NameGuid);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Получить SQL-запрос, проверяющий заполненность свойства-коллекции в базе данных системы.
    /// </summary>
    /// <param name="collection">Метаданные свойства-колекции.</param>
    /// <param name="resources">Системные ресурсы.</param>
    /// <returns>SQL-запрос</returns>
    private static string GetSQLRequestCheckingFullnessEntityColliectionProperty(EntityMetadata collection, IEnumerable<Tuple<ResourcesCollection, string>> resources)
    {
      var entity = collection.ActualMainCollectionPropertyMetadata.ParentEntityMetadata;
      var collectionLocalization = ReportGenerator.GetLocalizationString(resources, entity.Name, $"Property_{collection.ActualMainCollectionPropertyMetadata.Name}");
      var entityLocalization = ReportGenerator.GetLocalizationString(resources, entity.Name, "DisplayName") ??
        ReportGenerator.GetLocalizationString(resources, entity.Name, "AccusativeDisplayName");

      StringBuilder sb = new StringBuilder();
      sb.Append($"\n-- ###{collection.ActualMainCollectionPropertyMetadata.Name}\n");
      sb.AppendFormat(CollectionAddRequest, entity.Name, entityLocalization, entity.DBTableName, collection.Name,
        collectionLocalization, collection.DBTableName, 1, entity.NameGuid, entity.BaseRootEntityMetadata.Code);
      return sb.ToString();
    }

    /// <summary>
    /// Получить SQL-запрос, проверяющий заполненность свойства свойства-коллекции в базе данных системы.
    /// </summary>
    /// <param name="collectionSubProperty">Метаданные свойства свойства-колекции.</param>
    /// <param name="resources">Системные ресурсы.</param>
    /// <returns>SQL-запрос</returns>
    private static string GetSQLRequestCheckingFullnessColliectionProperty(PropertyMetadata collectionSubProperty, IEnumerable<Tuple<ResourcesCollection, string>> resources)
    {
      var collection = collectionSubProperty.ParentEntityMetadata;
      var entity = collection.ActualMainCollectionPropertyMetadata.ParentEntityMetadata;
      var collectionLocalization = ReportGenerator.GetLocalizationString(resources, entity.Name, $"Property_{collection.ActualMainCollectionPropertyMetadata.Name}");
      var entityLocalization = ReportGenerator.GetLocalizationString(resources, entity.Name, "DisplayName") ??
        ReportGenerator.GetLocalizationString(resources, entity.Name, "AccusativeDisplayName");

      StringBuilder sb = new StringBuilder();
      sb.Append($"\n-- ####{collectionSubProperty.Name}\n");
      if (collectionSubProperty.IsAncestorMetadata)
      {
        var ancestorSubProperty = GetAncestorProperty(collectionSubProperty);
        var subPropertyLocalization = ReportGenerator.GetLocalizationString(resources, ancestorSubProperty.ParentEntityMetadata.Name, $"Property_{collectionSubProperty.Name}");
        sb.AppendFormat(SubPropertyAddRequest, entity.Name, entityLocalization, entity.DBTableName, collection.ActualMainCollectionPropertyMetadata.Name,
          collectionLocalization, collection.DBTableName, 1, collectionSubProperty.Name, subPropertyLocalization, ancestorSubProperty.DBColumnName, collection.NameGuid);
      }
      else
      {
        var subPropertyLocalization = ReportGenerator.GetLocalizationString(resources, collection.Name, $"Property_{collectionSubProperty.Name}");
        sb.AppendFormat(SubPropertyAddRequest, entity.Name, entityLocalization, entity.DBTableName, collection.ActualMainCollectionPropertyMetadata.Name,
          collectionLocalization, collection.DBTableName, 1, collectionSubProperty.Name, subPropertyLocalization, collectionSubProperty.DBColumnName, collection.NameGuid);
      }
      return sb.ToString();
    }
  }
}