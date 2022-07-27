using System;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;

namespace ResolveReferences.Moqs
{
  public class UsersInfo : IUserInfo
  {
    Moq.Mock<IUserInfo> moq;

    public UsersInfo()
    {
      moq = new Moq.Mock<IUserInfo>();
      moq.Setup(f => f.Properties.Name.Name).Returns("Name");
    }

    public AccessRightsMode AccessRightsMode
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public IUserActionsInfo Actions
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

    public IUserPropertiesInfo Properties
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

    IRecipientActionsInfo IRecipientInfo.Actions
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

    IDatabookEntryPropertiesInfo IDatabookEntryInfo.Properties
    {
      get
      {
        return moq.Object.Properties;
      }
    }

    IRecipientPropertiesInfo IRecipientInfo.Properties
    {
      get
      {
        return moq.Object.Properties;
      }
    }
  }
}
