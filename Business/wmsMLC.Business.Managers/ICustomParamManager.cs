using System;
using System.Collections.Generic;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public interface ICustomParamManager
    {
        IEnumerable<CustomParam> GetByEntity(Type entityType, string sourceTypeName = null, string targetTypeName = null);
        IEnumerable<CustomParam> GetCPByInstance(string entity, string key, string attrentity = null, string cpSource = null, string cpTarget = null);
    }
}
