using LocalizationHelper.Resx;
using Sungero.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataParser
{
  public static class MetadataParser
  {
    #region Поля и свойства

    public const string LocalizationDescriptionFormat = "en - {0} | ru - {1}";

    public enum RusPropertyType
    {
      Строка = 0,
      Целое = 1,
      Вещественное = 2,
      Логическое = 3,
      Collection = 4,
      Перечисление = 5,
      Ссылка = 6,
      Дата = 7,
      Component = 10,
      Guid = 11,
      БинарныеДанныеВХранилище = 12,
      Текст = 13,
      БинарныеДанные = 14,
      Картинка = 15
    }

    private static RusPropertyType GetRusPropertyType(Sungero.Metadata.PropertyType propertyType)
    {
      var rusPropertyType = RusPropertyType.Строка;
      switch (propertyType)
      {
        case PropertyType.Integer:
          rusPropertyType = RusPropertyType.Целое;
          break;

        case PropertyType.Double:
          rusPropertyType = RusPropertyType.Вещественное;
          break;
        case PropertyType.Boolean:
          rusPropertyType = RusPropertyType.Логическое;
          break;
        case PropertyType.Collection:
          rusPropertyType = RusPropertyType.Collection;
          break;
        case PropertyType.Enumeration:
          rusPropertyType = RusPropertyType.Перечисление;
          break;
        case PropertyType.Navigation:
          rusPropertyType = RusPropertyType.Ссылка;
          break;
        case PropertyType.DateTime:
          rusPropertyType = RusPropertyType.Дата;
          break;
        case PropertyType.Component:
          rusPropertyType = RusPropertyType.Component;
          break;
        case PropertyType.Guid:
          rusPropertyType = RusPropertyType.Guid;
          break;
        case PropertyType.BinaryData:
          rusPropertyType = RusPropertyType.БинарныеДанныеВХранилище;
          break;

        case PropertyType.Text:
          rusPropertyType = RusPropertyType.Текст;
          break;
        case PropertyType.Data:
          rusPropertyType = RusPropertyType.БинарныеДанные;
          break;
        case PropertyType.Image:
          rusPropertyType = RusPropertyType.Картинка;
          break;
      }
      return rusPropertyType;
    }

    #endregion

    #region Описание

    /// <summary>
    /// Получить описание модуля.
    /// </summary>
    /// <param name="module">Метаданные модуля.</param>
    /// <returns>Описание модуля.</returns>
    public static List<string> GetModuleDescription(ModuleMetadata module)
    {
      var description = new List<string>();
      description.Add($"# {module.Name}\n");
      return description;
    }

    /// <summary>
    /// Получить описание сущности.
    /// </summary>
    /// <param name="entity">Метаданные сущности.</param>
    /// <param name="resources">Системные ресурсы.</param>
    /// <returns>Описание сущности.</returns>
    public static List<string> GetEntityDescription(EntityMetadata entity,
      IEnumerable<Tuple<ResourcesCollection, string>> resources)
    {
      var description = new List<string>();
      description.Add($"## {entity.Name}\n");
      var entityInfo = $"{ entity.Name }: { entity.Code } | { GetEntityLocalizationDescription(resources, entity, "DisplayName") }";
      description.Add(entityInfo);
      description.Add(string.Empty);
      return description;
    }

    /// <summary>
    /// Получить описание свойств-коллекций.
    /// </summary>
    /// <param name="collectionProperties">Свойства-коллекции.</param>
    /// <param name="resources">Системные ресурсы.</param>
    /// <returns>Описание свойств-коллекций.</returns>
    public static List<string> GetCollectionPropertiesDescription(List<EntityMetadata> collectionProperties,
      IEnumerable<Tuple<ResourcesCollection, string>> resources)
    {
      var propertiesText = new List<string>();
      foreach (EntityMetadata property in collectionProperties)
      {
        propertiesText.Add($"### Свойство-коллекция {property.Name}\n");

        // Добавить описание свойства-коллекции.
        propertiesText.Add($"{ property.ActualMainCollectionPropertyMetadata.Name }: { property.Code } " +
          $"| { property.DBTableName } " +
          $"| { GetEntityLocalizationDescription(resources, property, "DisplayName") }");
        propertiesText.Add(string.Empty);

        // Добавить описание свойств.
        propertiesText.Add("*ИД (Id) | Целое");
        propertiesText.Add($"Discriminator (Discriminator) | uniqueidentifier | {property.NameGuid.ToString().ToUpper()}");
        propertiesText.AddRange(GetPropertiesDescription(property, resources));
        propertiesText.Add(string.Empty);
      }
      return propertiesText;
    }

    /// <summary>
    /// Получить описание свойств сущности, кроме коллекций.
    /// </summary>
    /// <param name="entity">Метаданные сущности.</param>
    /// <param name="resources">Системные ресурсы.</param>
    /// <returns>Описание свойств сущности, кроме коллекций.</returns>
    public static List<string> GetPropertiesDescription(EntityMetadata entity, IEnumerable<Tuple<ResourcesCollection, string>> resources)
    {
      var propertiesText = new List<string>();
      // Свойства, кроме коллекций и перечислений. Не выводить информацию по свойствам, добавленным в предках.
      var properties = entity?.Properties?.Where(x => !x.IsAncestorMetadata);
      if (properties != null && properties.Any())
      {
        propertiesText.Add("### Свойства:");
        //var propertiesDescription = properties
        //  .Where(x => !(x is CollectionPropertyMetadata) && !(x is EnumPropertyMetadata))
        //  .Select(x => $"{ x.Name }: { x.PropertyType }{ GetPropertyAdditionalInfo(x) } | " +
        //               $"{ x.DBColumnName } | { GetPropertyLocalizationDescription(resources, entity.Name, x.Name)}");

        var propertiesDescription = properties
          .Where(x => !(x is CollectionPropertyMetadata) && !(x is EnumPropertyMetadata))
          .Select(x => $"{(x.IsRequired ? "*" : "")}" +
                       $"{(string.IsNullOrEmpty(GetPropertyAnyLocalizationDescription(resources, entity.Name, x.Name)) ? x.Name : GetPropertyAnyLocalizationDescription(resources, entity.Name, x.Name))} " +
                       $"({x.DBColumnName}) | {GetRusPropertyType(x.PropertyType)}{GetPropertyAdditionalInfo(x)}");

        propertiesText.AddRange(propertiesDescription);

        // Свойства-перечисления.
        var enumProperties = properties.Where(x => x is EnumPropertyMetadata).Select(x => x as EnumPropertyMetadata);
        if (enumProperties.Any())
          propertiesText.Add("### Свойства-перечисления:");
        foreach (var property in enumProperties)
        {
          var propertyDescription = GetPropertyAnyLocalizationDescription(resources, entity.Name, property.Name);
          propertiesText.Add(string.Empty);
          propertiesText.Add($"{(property.IsRequired ? "*" : "")}" +
                             $"{(string.IsNullOrEmpty(propertyDescription) ? property.Name : propertyDescription)} " +
                             $"({property.DBColumnName}) | {GetRusPropertyType(property.PropertyType)}");
          propertiesText.Add(string.Empty);

          var enumvaluesDescription = property.Values
            .Select(x => $"{GetEnumValueRussianDescription(resources, entity.Name, property.Name, x.Name)} ({x.Name})");
          propertiesText.AddRange(enumvaluesDescription);
        }
      }

      return propertiesText;
    }

    /// <summary>
    /// Получить описание для типа свойства.
    /// </summary>
    /// <param name="property">Свойство.</param>
    /// <returns>Описание типа свойства.</returns>
    /// <remarks>Для типа Navigation будет возвращен тип сущности, на который ссылается свойство-ссылка.
    /// Для типа String - длина строкового свойства.</remarks>
    public static string GetPropertyAdditionalInfo(PropertyMetadata property)
    {
      if (property is StringPropertyMetadata)
        return $" ({ ((StringPropertyMetadata)property).Length })";

      if (property is NavigationPropertyMetadata)
      {
        var navigation = (NavigationPropertyMetadata)property;
        return !string.IsNullOrWhiteSpace(navigation.InterfaceMetadata?.FullName)
          ? $" ({ navigation.InterfaceMetadata?.FullName })"
          : string.Empty;
      }

      return string.Empty;
    }

    /// <summary>
    /// Проверить, являются ли метаданные сущности метаданными свойства-коллекции.
    /// </summary>
    /// <param name="entity">Метаданные сущности.</param>
    /// <returns>True, если метаданные свойства-коллекции, иначе - False.</returns>
    public static bool IsCollectionProperty(EntityMetadata entity)
    {
      try
      {
        var actualMainEntityMetadata = entity.ActualMainEntityMetadata;
        var actualMainCollectionPropertyMetadata = entity.ActualMainCollectionPropertyMetadata;
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Получить описание структур.
    /// </summary>
    /// <param name="structures">Коллекция метаданных структур.</param>
    /// <param name="isModule">True - структуры модуля, false - структуры сущности.</param>
    /// <returns>Описание структур.</returns>
    public static List<string> GetStructuresDescription(System.Collections.ObjectModel.ObservableCollection<StructureMetadata> structures,
                                                        bool isModule)
    {
      var structuresDescription = new List<string>();

      foreach (var structure in structures)
      {
        // Заголовок.
        if (isModule)
          structuresDescription.Add($"## Структура модуля {structure.Name}. Свойства:\n");
        else
          structuresDescription.Add($"### Структура сущности {structure.Name}. Свойства:\n");

        // Свойства.
        var properties = structure?.Properties?.OrderBy(x => x.Name);
        if (properties != null && properties.Any())
        {
          var propertiesDescription = properties
            .Select(x => $"{ x.Name }: { x.TypeFullName }");
          structuresDescription.AddRange(propertiesDescription);
        }

        // Пустая строка между структурами.
        structuresDescription.Add(string.Empty);
      }

      return structuresDescription;
    }

    /// <summary>
    /// Получить описание констант.
    /// </summary>
    /// <param name="constants">Коллекция метаданных констант.</param>
    /// <param name="isModule">True - константы модуля, false - константы сущности.</param>
    /// <returns>Описание констант.</returns>
    public static List<string> GetConstantsDescription(System.Collections.ObjectModel.ObservableCollection<ConstantMetadata> constants,
                                                        bool isModule)
    {
      var constantsDescription = new List<string>();

      if (constants.Count > 0)
      {
        // Заголовок.
        if (isModule)
          constantsDescription.Add($"## Константы модуля:\n");
        else
          constantsDescription.Add($"### Константы сущности:\n");

        // Сигнатура констант.
        // ParentClasses нужен для сгруппированных констант.
        var constantsSignature = constants
          .Select(x => $"{String.Join(".", x.ParentClasses)}: { x.TypeName } { x.Name } = { x.Value}");
        constantsDescription.AddRange(constantsSignature);

        // Пустая строка.
        constantsDescription.Add(string.Empty);
      }

      return constantsDescription;
    }

    #endregion

    #region Локализация

    /// <summary>
    /// Получить отображаемое имя сущности или свойства-коллекции в формате "en - {} | ru - {}".
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="entity"></param>
    /// <param name="CodeName"></param>
    /// <returns>Информация об отображаемом имени.</returns>
    public static string GetEntityLocalizationDescription(IEnumerable<Tuple<ResourcesCollection, string>> resources,
      EntityMetadata entity, string CodeName)
    {
      var localizationString = GetSystemResource(resources, entity.Name, CodeName) ??
        GetSystemResource(resources, entity.BaseEntityMetadata.Name, CodeName);

      // Попытаться взять локализацию свойства-коллекции.
      if (localizationString == null)
      {
        try
        {
          localizationString = GetSystemResource(resources, entity.ActualMainEntityMetadata?.Name,
          $"Property_{entity.ActualMainCollectionPropertyMetadata?.Name}");
        }
        catch
        {
          return string.Empty;
        }
      }

      return localizationString != null 
        ? string.Format(LocalizationDescriptionFormat, localizationString?.Default, localizationString?.Russian)
        : string.Empty;
    }

    /// <summary>
    /// Получить отображаемое имя свойства в формате "en - {} | ru - {}".
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="entityName">Наименование сущности или свойства-коллекции, в ресурсах которого производится поиск.</param>
    /// <param name="propertyCode">Код свойства.</param>
    /// <returns></returns>
    public static string GetPropertyLocalizationDescription(IEnumerable<Tuple<ResourcesCollection, string>> resources, string entityName, string propertyCode)
    {
      var localizationString = GetSystemResource(resources, entityName, $"Property_{ propertyCode }");
      return localizationString != null
        ? string.Format(LocalizationDescriptionFormat, localizationString?.Default, localizationString?.Russian)
        : string.Empty;
    }

    /// <summary>
    /// Получить отображаемое имя свойства в русской локализации.
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="entityName">Наименование сущности или свойства-коллекции, в ресурсах которого производится поиск.</param>
    /// <param name="propertyCode">Код свойства.</param>
    /// <returns></returns>
    public static string GetPropertyRussianDescription(IEnumerable<Tuple<ResourcesCollection, string>> resources, string entityName, string propertyCode)
    {
      var localizationString = GetSystemResource(resources, entityName, $"Property_{ propertyCode }");
      return localizationString != null
        ? localizationString?.Russian
        : string.Empty;
    }

    /// <summary>
    /// Получить отображаемое имя свойства в локализации.
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="entityName">Наименование сущности или свойства-коллекции, в ресурсах которого производится поиск.</param>
    /// <param name="propertyCode">Код свойства.</param>
    /// <returns></returns>
    public static string GetPropertyAnyLocalizationDescription(IEnumerable<Tuple<ResourcesCollection, string>> resources, string entityName, string propertyCode)
    {
      var result = string.Empty;
      var localizationString = GetSystemResource(resources, entityName, $"Property_{ propertyCode }");
      if (localizationString != null)
      {
        result = localizationString?.Russian;
        if (string.IsNullOrWhiteSpace(result))
          result = localizationString?.Default;
      }
      return result;
    }

    /// <summary>
    /// Получить отображаемое имя значения свойства-перечисления в формате "en - {} | ru - {}".
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="entityName">Наименование сущности или свойства-коллекции, в ресурсах которого производится поиск.</param>
    /// <param name="propertyName">Наименование свойства.</param>
    /// <param name="enumValueCode">Код свойства.</param>
    /// <returns></returns>
    public static string GetEnumValueLocalizationDescription(IEnumerable<Tuple<ResourcesCollection, string>> resources, string entityName, string propertyName, string enumValueCode)
    {
      var localizationString = GetSystemResource(resources, entityName, $"Enum_{ propertyName }_{ enumValueCode }");
      return localizationString != null
        ? string.Format(LocalizationDescriptionFormat, localizationString?.Default, localizationString?.Russian)
        : string.Empty;
    }

    /// <summary>
    /// Получить отображаемое имя значения свойства-перечисления в русской локализации.
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="entityName">Наименование сущности или свойства-коллекции, в ресурсах которого производится поиск.</param>
    /// <param name="propertyName">Наименование свойства.</param>
    /// <param name="enumValueCode">Код свойства.</param>
    /// <returns></returns>
    public static string GetEnumValueRussianDescription(IEnumerable<Tuple<ResourcesCollection, string>> resources, string entityName, string propertyName, string enumValueCode)
    {
      var localizationString = GetSystemResource(resources, entityName, $"Enum_{ propertyName }_{ enumValueCode }");
      return localizationString != null
        ? localizationString?.Russian
        : string.Empty;
    }

    /// <summary>
    /// Получить системный ресурс со строкой локализации.
    /// </summary>
    /// <param name="resources">Системные ресурсы.</param>
    /// <param name="mtdName">Наименование файла с mtd, к которому привязаны ресурсы.</param>
    /// <param name="entityName">Наименование сущности или свойства-коллекции, в ресурсах которого производится поиск.</param>
    /// <returns>Системный ресурс со строкой локализации.</returns>
    public static ResxLine GetSystemResource(IEnumerable<Tuple<ResourcesCollection, string>> resources,
      string mtdName, string entityName)
    {
      return resources.Where(s => s.Item2 == $"{ mtdName }.mtd")
      .Select(s => s.Item1.Where(sys => sys.Code == entityName).FirstOrDefault())
      .FirstOrDefault();
    }

    #endregion

  }
}