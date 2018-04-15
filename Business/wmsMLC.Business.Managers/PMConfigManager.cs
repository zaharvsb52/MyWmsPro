using System.Collections.Generic;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public class PMConfigManager : WMSBusinessObjectManager<PMConfig, decimal>, IPMConfigManager
    {
        //TODO: перенести в BPProcessManager
        public List<PMConfig> GetPMConfigByParamListByArtCode(string artCode, string operationCode, string methodName)
        {
            using (var repo = GetRepository<IPMConfigRepository>())
                return repo.GetPMConfigByParamListByArtCode(artCode, operationCode, methodName);
        }

        //TODO: перенести в BPProcessManager
        public List<PMConfig> GetPMConfigByParamListByProductId(decimal? productId, string operationCode, string methodName)
        {
            using (var repo = GetRepository<IPMConfigRepository>())
                return repo.GetPMConfigByParamListByProductId(productId, operationCode, methodName);
        }
    }
}
