using System;
using System.Collections.Generic;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class CustomParamManager : WMSBusinessObjectManager<CustomParam, string>, ICustomParamManager
    {
        public IEnumerable<CustomParam> GetByEntity(Type entityType, string sourceTypeName, string targetTypeName)
        {
            var entityName = SourceNameHelper.Instance.GetSourceName(entityType);
            var filter = string.Format("{0} = '{1}'", CustomParam.CustomParam2EntityPropertyName, entityName.ToUpper());

            if (!string.IsNullOrEmpty(sourceTypeName))
                filter = string.Format("{0} AND {1} = '{2}'", filter, CustomParam.CUSTOMPARAMSOURCEPropertyName, sourceTypeName);

            if (!string.IsNullOrEmpty(targetTypeName))
                filter = string.Format("{0} AND {1} = '{2}'", filter, CustomParam.CUSTOMPARAMTARGETPropertyName, targetTypeName);

            using (var repo = GetRepository())
                return repo.GetFiltered(filter, null);
        }

        public IEnumerable<CustomParam> GetCPByInstance(string entity, string key, string attrentity = null, string cpSource = null, string cpTarget = null)
        {
            using (var repo = GetRepository<ICustomParamRepository>())
                return repo.GetCPByInstance(entity, key, attrentity, cpSource, cpTarget);
        }
    }
}
