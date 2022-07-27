using System;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;

namespace ResolveReferences.Moqs
{
  class DatabookHistoryInfo : IDatabookHistoryInfo
  {
    Moq.Mock<IDatabookHistoryInfo> moq;

    public DatabookHistoryInfo()
    {
      moq = new Moq.Mock<IDatabookHistoryInfo>();
      moq.Setup(f => f.Properties.Comment.Name).Returns(nameof(Properties.Comment));
      moq.Setup(f => f.Properties.HistoryDate.Name).Returns(nameof(Properties.HistoryDate));
      moq.Setup(f => f.Properties.User.Name).Returns(nameof(Properties.User));
      moq.Setup(f => f.Properties.EntityId.Name).Returns(nameof(Properties.EntityId));
      moq.Setup(f => f.Properties.EntityType.Name).Returns(nameof(Properties.EntityType));
      moq.Setup(f => f.Properties.Action.Name).Returns(nameof(Properties.Action));
      moq.Setup(f => f.Properties.IsSubstitute.Name).Returns(nameof(Properties.IsSubstitute));
      moq.Setup(f => f.Properties.HostName.Name).Returns(nameof(Properties.HostName));
    }

    public AccessRightsMode AccessRightsMode
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public IEntityActionsInfo Actions
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string DBTableName
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public IEntityEventsInfo Events
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public bool IsCacheable
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string LocalizedName
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string LocalizedPluralName
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string Name
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public IDatabookHistoryPropertiesInfo Properties
    {
      get
      {
        return moq.Object.Properties;
      }
    }

    IEntityPropertiesInfo IEntityInfo.Properties
    {
      get
      {
        return moq.Object.Properties;
      }
    }

    IHistoryPropertiesInfo IHistoryInfo.Properties
    {
      get
      {
        return moq.Object.Properties;
      }
    }
  }
}
