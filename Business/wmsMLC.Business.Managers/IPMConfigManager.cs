using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IPMConfigManager : IBaseManager<PMConfig, decimal>
    {
        List<PMConfig> GetPMConfigByParamListByArtCode(string artCode, string operationCode, string methodName);
        List<PMConfig> GetPMConfigByParamListByProductId(decimal? productId, string operationCode, string methodName);
    }
}
