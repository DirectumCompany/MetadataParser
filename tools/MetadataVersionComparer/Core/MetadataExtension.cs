using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using ResolveReferences;
using Sungero.Metadata;
using MetadataParser;

namespace MetadataVersionComparer.Core
{
  public static class MetadataExtension
  {
    public static string GetEntityName(this EntityMetadata metadata)
    {
      return metadata.IsChildEntity ? metadata.MainEntityMetadata.Name : metadata.Name;
    }

    public static string GetChildName(this EntityMetadata metadata)
    {
      return metadata.IsChildEntity ? metadata.Name : string.Empty;
    }

    public static ModuleItemMetadata GetThisChild(this IEnumerable<ModuleMetadata> modules, ModuleItemMetadata child)
    {
      var module = modules.SingleOrDefault(m => m.NameGuid == child.ModuleMetadata.NameGuid);
      if (module == null)
        return null;

      var oldChild = module.Children.Cast<ModuleItemMetadata>().SingleOrDefault(c => c.NameGuid == child.NameGuid);

      return oldChild;
    }

    public static ModuleMetadata GetThisChild(this IEnumerable<ModuleMetadata> modules, ModuleMetadata child)
    {
      return modules.SingleOrDefault(m => m.NameGuid == child.NameGuid);
    }

    public static PropertyMetadata GetThisProperty(this ModuleItemMetadata item, PropertyMetadata property)
    {
      var entity = item as EntityMetadata;
      if (entity == null)
        return null;

      var oldProperty = entity.Properties.SingleOrDefault(p => p.NameGuid == property.NameGuid);
      return oldProperty;
    }

    public static FunctionMetadata GetThisFunction(this IEnumerable<FunctionMetadata> items, FunctionMetadata function)
    {
      // Ищем по имени, т.к. guid почему-то может меняться.
      return items.SingleOrDefault(p => p.Name == function.Name && p.ReturnType == function.ReturnType &&
        p.Parameters.Select(pr => pr.ParameterType).SequenceEqual(function.Parameters.Select(pr => pr.ParameterType)));
    }

    public static bool NavigationPropertyEntityChanged(this PropertyMetadata one, PropertyMetadata two)
    {
      return one is NavigationPropertyMetadata && two is NavigationPropertyMetadata &&
          ((NavigationPropertyMetadata)one).EntityGuid != ((NavigationPropertyMetadata)two).EntityGuid;
    }

    public static string GetNavigationPropertyEntityName(this PropertyMetadata property)
    {
      if (!(property is NavigationPropertyMetadata))
        return string.Empty;

      var navigation = (NavigationPropertyMetadata)property;
      return !string.IsNullOrWhiteSpace(navigation.InterfaceMetadata?.FullName)
        ? $" ({ navigation.InterfaceMetadata?.FullName })"
        : string.Empty;
    }

    public static bool StringPropertyLengthChanged(this PropertyMetadata one, PropertyMetadata two)
    {
      return one.GetStringPropertyLength() != two.GetStringPropertyLength();
    }

    public static int GetStringPropertyLength(this PropertyMetadata property)
    {
      if (!(property is StringPropertyMetadata))
        return -1;

      return ((StringPropertyMetadata)property).Length;
    }

    public static IEnumerable<Diff> EnumerationChanges(this PropertyMetadata property, PropertyMetadata oldProperty)
    {
      var newEnum = property as EnumPropertyMetadata;
      var oldEnum = oldProperty as EnumPropertyMetadata;
      if (newEnum != null && oldEnum != null)
      {
        foreach (var value in oldEnum.DirectValues)
        {
          var nowValue = newEnum.DirectValues.SingleOrDefault(v => v.Code == value.Code);
          if (nowValue == null)
            yield return new PropertyChangedDiff(DiffType.Delete, oldProperty.ParentEntityMetadata.GetEntityName(), oldProperty.ParentEntityMetadata.GetChildName(), oldProperty.Name, string.Empty, value.Code);
        }

        foreach (var value in newEnum.DirectValues)
        {
          var oldValue = oldEnum.DirectValues.SingleOrDefault(v => v.Code == value.Code);
          if (oldValue == null)
            yield return new PropertyChangedDiff(DiffType.Add, property.ParentEntityMetadata.GetEntityName(), property.ParentEntityMetadata.GetChildName(), property.Name, value.Code, string.Empty);
        }
      }
    }

    /// <summary>
    /// Сравнить две сущности.
    /// </summary>
    /// <param name="newItem">Новая сущность.</param>
    /// <param name="oldItem">Старая сущность (null, если её и не было).</param>
    /// <returns></returns>
    public static IEnumerable<Diff> Compare(this MetadataBase newItem, MetadataBase oldItem)
    {
      // Добавление новой сущности.
      if (oldItem == null)
      {
        var em = newItem as EntityMetadata;
        if (em != null && em.IsChildEntity)
          yield return new EntityDiff(DiffType.Add, em.MainEntityMetadata.Name, em.Name);
        else
          yield return new EntityDiff(DiffType.Add, newItem.Name, string.Empty);
      }

      var entityMetadata = newItem as EntityMetadata;
      var oldEntityMetadata = oldItem as EntityMetadata;
      if (entityMetadata != null)
      {
        foreach (var diff in entityMetadata.Compare(oldEntityMetadata))
          yield return diff;
      }
    }

    private static IEnumerable<Diff> Compare(this EntityMetadata entityMetadata, EntityMetadata oldEntityMetadata)
    {
      var entity = entityMetadata.GetEntityName();
      var child = entityMetadata.GetChildName();

      // Удаленные свойства.
      if (oldEntityMetadata != null)
      {
        foreach (var property in oldEntityMetadata.Properties)
        {
          var newProperty = entityMetadata.GetThisProperty(property);
          if (newProperty == null)
          {
            var navigation = property as NavigationPropertyMetadata;
            if (navigation != null && navigation.IsReferenceToRootEntity)
              continue;

            yield return new PropertyDiff(DiffType.Delete, entity, child, property.Name);
          }
        }
      }

      foreach (var property in entityMetadata.Properties)
      {
        var oldProperty = oldEntityMetadata.GetThisProperty(property);

        // Новые свойства.
        if (oldProperty == null)
        {
          var navigation = property as NavigationPropertyMetadata;
          if (navigation != null)
          {
            if (navigation.IsReferenceToRootEntity)
              continue;

            yield return new NavigationPropertyDiff(DiffType.Add, entity, child, navigation.Name, navigation.InterfaceMetadata.FullName);
          }
          else
            yield return new PropertyDiff(DiffType.Add, entity, child, property.Name);
        }

        if (oldProperty == null)
          continue;

        // Изменение типа свойства (строка в текст например).
        if (oldProperty.PropertyType != property.PropertyType)
          yield return new PropertyTypeChangedDiff(DiffType.Edit, entity, child, property.Name, property.PropertyType, oldProperty.PropertyType);

        // Изменение свойства ссылки.
        if (oldProperty.NavigationPropertyEntityChanged(property))
          yield return new PropertyChangedDiff(DiffType.Edit, entity, child, property.Name, property.GetNavigationPropertyEntityName(), oldProperty.GetNavigationPropertyEntityName());

        // Изменение длины строки.
        if (oldProperty.StringPropertyLengthChanged(property))
          yield return new StringLengthDiff(DiffType.Edit, entity, child, property.Name, property.GetStringPropertyLength(), oldProperty.GetStringPropertyLength());

        // Изменение состава перечислений.
        foreach (var diff in EnumerationChanges(property, oldProperty))
        {
          yield return diff;
        }

        // Переименовали свойство (или сменили ему код).
        if (oldProperty.Name != property.Name || oldProperty.Code != property.Code)
          yield return new PropertyRenamedDiff(DiffType.Edit, entity, child, property, oldProperty);

        // Изменили настройки свойства (обязательность, уникальность и т.д.).
        if (oldProperty.IsRequired != property.IsRequired)
        {
          if (property.IsRequired)
            yield return new PropertyRequiredDiff(DiffType.Edit, entity, child, property, oldProperty);
          else
            yield return new PropertyRequiredDiff(DiffType.Edit, entity, child, property, oldProperty);
        }

      }
    }

    private static IEnumerable<Diff> CompareFunctions<T>(this T metadata, T oldMetadata, Func<T, ObservableCollection<FunctionMetadata>> functionSelector) where T : MetadataBase
    {
      var metadataFunctions = functionSelector.Invoke(metadata);
      var oldFunctions = oldMetadata != null ? functionSelector.Invoke(oldMetadata) : Enumerable.Empty<FunctionMetadata>();
      foreach (var function in metadataFunctions)
      {
        // Если сущности не было в старой версии - однозначно новая функция.
        // Если не нашлась такая же сигнатура в старой версии - тоже считаем новой.
        if (oldMetadata == null || oldFunctions.GetThisFunction(function) == null)
          yield return new FunctionDiff(DiffType.Add, metadata.Name, function);
      }

      // Удаленные публичные функции (возможно, нам вообще нельзя больше удалять и их стоит оставлять с атрибутом Obsolete?).
      if (oldMetadata != null)
      {
        foreach (var function in oldFunctions)
        {
          var newFunction = metadataFunctions.GetThisFunction(function);
          if (newFunction == null)
            yield return new FunctionDiff(DiffType.Delete, metadata.Name, function);
        }
      }
    }
  }
}