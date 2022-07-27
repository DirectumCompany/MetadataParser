using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Metadata;

namespace MetadataVersionComparer.Core
{
  public abstract class Diff
  {
    public DiffType DiffType;

    public string Entity;

    public string Child;

    public string Module;

    public bool IsChild { get { return !string.IsNullOrWhiteSpace(Child); } }

    public abstract override string ToString();

    protected Diff(DiffType type, string entity, string child)
    {
      this.DiffType = type;
      this.Entity = entity;
      this.Child = child;
    }
  }

  public class EntityDiff : Diff
  {
    public override string ToString()
    {
      if (!IsChild)
      {
        switch (DiffType)
        {
          case DiffType.Add:
            return string.Format("- Добавлена сущность {0}", Entity);
          case DiffType.Edit:
            return string.Format("- Изменена сущность {0}", Entity);
          case DiffType.Delete:
            return string.Format("- Удалена сущность {0}", Entity);
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      else
      {
        switch (DiffType)
        {
          case DiffType.Add:
            return string.Format("- Добавлена дочерняя сущность {0}", Child);
          case DiffType.Edit:
            return string.Format("- Изменена дочерняя сущность {0}", Child);
          case DiffType.Delete:
            return string.Format("- Удалена дочерняя сущность {0}", Child);
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    public EntityDiff(DiffType type, string entity, string child) : base(type, entity, child)
    {
    }
  }

  public class PropertyDiff : Diff
  {
    public string Property;

    public override string ToString()
    {
      switch (DiffType)
      {
        case DiffType.Add:
          return string.Format("- Добавлено свойство {0}", Property);
        case DiffType.Edit:
          return string.Format("- Изменено свойство {0}", Property);
        case DiffType.Delete:
          return string.Format("- Удалено свойство {0}", Property);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public PropertyDiff(DiffType type, string entity, string child, string property) : base(type, entity, child)
    {
      this.Property = property;
    }
  }

  public class NavigationPropertyDiff : PropertyDiff
  {
    public string ToProperty;

    public override string ToString()
    {
      switch (DiffType)
      {
        case DiffType.Add:
          return string.Format("- Добавлено свойство {0}, ссылается на {1}", Property, ToProperty);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public NavigationPropertyDiff(DiffType type, string entity, string child, string property, string toProperty) : base(type, entity, child, property)
    {
      this.ToProperty = toProperty;
    }
  }

  public abstract class PropertyDiff<T> : PropertyDiff
  {
    public T NewValue;

    public T OldValue;

    public override string ToString()
    {
      switch (DiffType)
      {
        case DiffType.Add:
          return string.Format("- В свойство {0} добавлено значение {1}", Property, NewValue);
        case DiffType.Edit:
          return string.Format("- Свойство {0} изменено с {1} на {2}", Property, OldValue, NewValue);
        case DiffType.Delete:
          return string.Format("- Из свойства {0} удалено значение {1}", Property, OldValue);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    protected PropertyDiff(DiffType type, string entity, string child, string property, T newValue, T oldValue) : base(type, entity, child, property)
    {
      this.NewValue = newValue;
      this.OldValue = oldValue;
    }
  }

  public class PropertyChangedDiff : PropertyDiff<string>
  {
    public PropertyChangedDiff(DiffType type, string entity, string child, string property, string newValue, string oldValue) : base(type, entity, child, property, newValue, oldValue)
    {
    }
  }

  public class PropertyTypeChangedDiff : PropertyDiff<PropertyType>
  {
    public PropertyTypeChangedDiff(DiffType type, string entity, string child, string property, PropertyType newValue, PropertyType oldValue) : base(type, entity, child, property, newValue, oldValue)
    {
    }
  }

  public class PropertyRenamedDiff : PropertyDiff<PropertyMetadata>
  {
    public override string ToString()
    {
      switch (DiffType)
      {
        case DiffType.Edit:
          return string.Format("- Свойство {0} переименовано с {1} на {2} (и его код переименован с {3} на {4})", 
            Property, OldValue.Name, NewValue.Name, OldValue.Code, NewValue.Code);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public PropertyRenamedDiff(DiffType type, string entity, string child, PropertyMetadata newValue, PropertyMetadata oldValue) : 
      base(type, entity, child, oldValue.Name, newValue, oldValue)
    {
    }
  }

  public class PropertyRequiredDiff : PropertyDiff<PropertyMetadata>
  {
    public override string ToString()
    {
      switch (DiffType)
      {
        case DiffType.Edit:
          return string.Format("- Свойство {0} стало обязательным", Property);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public PropertyRequiredDiff(DiffType type, string entity, string child, PropertyMetadata newValue, PropertyMetadata oldValue) :
      base(type, entity, child, oldValue.Name, newValue, oldValue)
    {
    }
  }

  public class PropertyNotRequiredDiff : PropertyDiff<PropertyMetadata>
  {
    public override string ToString()
    {
      switch (DiffType)
      {
        case DiffType.Edit:
          return string.Format("- Свойство {0} стало не обязательным", Property);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public PropertyNotRequiredDiff(DiffType type, string entity, string child, PropertyMetadata newValue, PropertyMetadata oldValue) :
      base(type, entity, child, oldValue.Name, newValue, oldValue)
    {
    }
  }

  public class StringLengthDiff : PropertyDiff<int>
  {
    public override string ToString()
    {
      return string.Format("- Длина строки {0} изменена с {1} на {2}", Property, OldValue, NewValue);
    }

    public StringLengthDiff(DiffType type, string entity, string child, string property, int newValue, int oldValue) : base(type, entity, child, property, newValue, oldValue)
    {
    }
  }

  public class FunctionDiff : Diff
  {
    public string Function;

    public IEnumerable<string> Parameters;

    public override string ToString()
    {
      var function = string.Format("{0}({1})", Function, string.Join(", ", Parameters));
      switch (DiffType)
      {
        case DiffType.Add:
          return string.Format("- Добавлена функция {0}", function);
        case DiffType.Edit:
          return string.Format("- Изменена функция {0}", function);
        case DiffType.Delete:
          return string.Format("- Удалена функция {0}", function);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public FunctionDiff(DiffType type, string entity, FunctionMetadata metadata) : base(type, entity, string.Empty)
    {
      this.Function = metadata.Name;
      this.Parameters = metadata.Parameters.Select(p => p.ParameterType.Replace("global::", ""));
    }
  }

  public enum DiffType
  {
    Add,
    Edit,
    Delete,
  }
}