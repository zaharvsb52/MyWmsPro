using System.Collections.Generic;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service.Telegrams;
using wmsMLC.General.DAL.Service;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class CustomParamRepository : HistoryAndFullCacheableRepository<CustomParam, string>, ICustomParamRepository
    {
        public IEnumerable<CustomParam> GetCPByInstance(string entity, string key, string attrentity, string cpSource, string cpTarget)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<CustomParam>), IsOut = true };
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(string), Value = entity, IsOut = false };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(string), Value = key, IsOut = false };
            var attrentityParam = new TransmitterParam { Name = "attrentity", Type = typeof(string), Value = attrentity, IsOut = false };
            var cpSourceParam = new TransmitterParam { Name = "cpSource", Type = typeof(string), Value = cpSource, IsOut = false };
            var cpTargetParam = new TransmitterParam { Name = "cpTarget", Type = typeof(string), Value = cpTarget, IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(CustomParam).Name, "GetCPByInstance", new[] { resultParam, entityParam, keyParam, attrentityParam, cpSourceParam, cpTargetParam });
            ProcessTelegramm(telegram);
            return (List<CustomParam>)resultParam.Value;
        }

    }
}