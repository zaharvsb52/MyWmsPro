using System;
using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class GlobalParamValueManager : WMSBusinessObjectManager<GlobalParamValue, decimal>, ISysDbInfo
    {
        private DbSysInfo _dbSystemInfo;
        public DbSysInfo DbSystemInfo
        {
            get { return _dbSystemInfo ?? (_dbSystemInfo = GetSysDbInfo()); }
        }

        private DbSysInfo GetSysDbInfo()
        {
            var result = new Dictionary<SysDbInfo, string>();
            var type = typeof(GlobalParamValue);
            var globalParamValues = GetFiltered(string.Format("{0} in ('{1}','{2}','{3}')",
                SourceNameHelper.Instance.GetPropertySourceName(type, GlobalParamValue.GlobalParamCodePropertyName),
                SysDbInfo.SysEnvironment, SysDbInfo.SysDBVersion, SysDbInfo.SysSite));

            foreach (var p in globalParamValues)
            {
                var key = p.GlobalParamCode_R.To(SysDbInfo.None);
                if (key == SysDbInfo.None)
                    continue;
                result[key] = p.GparamValValue;
            }

            Func<SysDbInfo, string> getresulthandler = key => result.ContainsKey(key) ? result[key] : null;

            return new DbSysInfo(version: getresulthandler(SysDbInfo.SysDBVersion), environment: getresulthandler(SysDbInfo.SysEnvironment),
                site: getresulthandler(SysDbInfo.SysSite));
        }
    }
}
