using System;
using System.Collections.Generic;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public interface IGlobalParamManager
    {
        IEnumerable<GlobalParam> GetByEntity(Type entityType);
    }
}