using System;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;

namespace ResolveReferences.Moqs
{
  class FolderInfo : IFolderInfo
  {
    Moq.Mock<IFolderInfo> moq;

    public FolderInfo()
    {
      moq = new Moq.Mock<IFolderInfo>();
      moq.Setup(f => f.Properties.Created.Name).Returns(nameof(Properties.Created));
      moq.Setup(f => f.Properties.Author.Name).Returns(nameof(Properties.Author));
      moq.Setup(f => f.Properties.Name.Name).Returns(nameof(Properties.Name));
      moq.Setup(f => f.Properties.IsSpecial.Name).Returns(nameof(Properties.IsSpecial));
      moq.Setup(f => f.Properties.MainEntityType.Name).Returns(nameof(Properties.MainEntityType));
      moq.Setup(f => f.Properties.SpecialFolderType.Name).Returns(nameof(Properties.SpecialFolderType));
    }

    public AccessRightsMode AccessRightsMode
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    IFolderBasePropertiesInfo IFolderBaseInfo.Properties
    {
      get { return Properties; }
    }

    public IFolderActionsInfo Actions
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

    public IFolderPropertiesInfo Properties
    {
      get
      {
        return moq.Object.Properties;
      }
    }

    IEntityActionsInfo IEntityInfo.Actions
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    IEntityPropertiesInfo IEntityInfo.Properties
    {
      get
      {
        return moq.Object.Properties;
      }
    }

    IFolderBaseActionsInfo IFolderBaseInfo.Actions
    {
      get
      {
        throw new NotImplementedException();
      }
    }
  }
}
