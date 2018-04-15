using System;
using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class GlobalParamManager : WMSBusinessObjectManager<GlobalParam, string>, IGlobalParamManager
    {
        public IEnumerable<GlobalParam> GetByEntity(Type entityType)
        {
            var entityName = SourceNameHelper.Instance.GetSourceName(entityType);
            var filter = string.Format("{0} = '{1}'", GlobalParam.GlobalParam2EntityPropertyName, entityName.ToUpper());
            using (var repo = GetRepository())
                return repo.GetFiltered(filter, null);
        }
    }
}
