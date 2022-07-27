using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolveReferences.Moqs
{
  class TenantInfo : Sungero.Domain.Shared.ITenantInfo
  {
    public string TenantCultureName
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string TenantId
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public TimeSpan UtcOffset
    {
      get
      {
        return new TimeSpan(4, 0, 0);
      }
    }
  }
}
