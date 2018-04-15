using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface ICustomParamRepository : IRepository<CustomParam, string>
    {
        IEnumerable<CustomParam> GetCPByInstance(string entity, string key, string attrentity, string cpSource, string cpTarget);
    }
}