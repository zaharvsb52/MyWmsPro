using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IPMConfigRepository : IRepository<PMConfig, decimal>
    {
        List<PMConfig> GetPMConfigByParamListByArtCode(string artCode, string operationCode, string methodName);
        List<PMConfig> GetPMConfigByParamListByProductId(decimal? productId, string operationCode, string methodName);
    }
}
