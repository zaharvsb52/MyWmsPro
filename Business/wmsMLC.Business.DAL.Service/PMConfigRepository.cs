using System.Collections.Generic;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class PMConfigRepository : BaseHistoryRepository<PMConfig, decimal>, IPMConfigRepository, ICacheableRepository
    {
        [Cache(MaxCacheTime = 600000, IsWeak = false)]
        public virtual List<PMConfig> GetPMConfigByParamListByArtCode(string artCode, string operationCode, string methodName)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<PMConfig>), IsOut = true };
            var artCodeParam = new TransmitterParam { Name = "artCode", Type = typeof(string), IsOut = false, Value = artCode };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var methodNameParam = new TransmitterParam { Name = "methodName", Type = typeof(string), IsOut = false, Value = methodName };
            var telegram = new RepoQueryTelegramWrapper(typeof(PMConfig).Name, "GetPMConfigByParamListByArtCode", new[] { resultParam, artCodeParam, operationCodeParam, methodNameParam });
            ProcessTelegramm(telegram);
            return (List<PMConfig>)resultParam.Value;
        }

        [Cache(MaxCacheTime = 600000, IsWeak = false)]
        public virtual List<PMConfig> GetPMConfigByParamListByProductId(decimal? productId, string operationCode, string methodName)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<PMConfig>), IsOut = true };
            var productIdParam = new TransmitterParam { Name = "productId", Type = typeof(decimal?), IsOut = false, Value = productId };
            var operationCodeParam = new TransmitterParam { Name = "operationCode", Type = typeof(string), IsOut = false, Value = operationCode };
            var methodNameParam = new TransmitterParam { Name = "methodName", Type = typeof(string), IsOut = false, Value = methodName };
            var telegram = new RepoQueryTelegramWrapper(typeof(PMConfig).Name, "GetPMConfigByParamListByProductId", new[] { resultParam, productIdParam, operationCodeParam, methodNameParam });
            ProcessTelegramm(telegram);
            return (List<PMConfig>)resultParam.Value;
        }

       public void ClearCache()
        {
            CacheAspect.ClearCache(GetType());
        }
    }
}
