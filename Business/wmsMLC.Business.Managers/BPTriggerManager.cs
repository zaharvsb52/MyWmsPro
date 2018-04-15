using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class BPTriggerManager : WMSBusinessObjectManager<BPTrigger, string>, IBPTriggerManager
    {
        public IEnumerable<BPTrigger> GetByEntity(Type type, BPTriggerMode triggerMode)
        {
            var strMode = triggerMode.ToString();
            var name = SourceNameHelper.Instance.GetSourceName(type);
            return GetAll(GetModeEnum.Partial).Where(i => strMode.EqIgnoreCase(i.TriggerMode) && name.EqIgnoreCase(i.OBJECTNAME_R));

            //ЗДЕСЬ НЕ ДОЛЖНО БЫТЬ SOURCENAME!
//            var filter = string.Format("({0} = '{1}' and {2}='{3}')", BPTrigger.ObjectNamePropertyName.ToUpper(), SourceNameHelper.Instance.GetSourceName(type).ToUpper(), BPTrigger.TriggerModePropertyName, triggerMode);
//            using (var repo = GetRepository())
//                return repo.GetFiltered(filter, null);
        }
    }
}
