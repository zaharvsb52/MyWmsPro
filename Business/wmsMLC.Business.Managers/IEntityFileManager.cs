using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IEntityFileManager : IBaseManager<EntityFile>
    {
        string GetFileBodyByEntity(string pEntity, string pKey);
        void SetFileBody(string pEntity, string pKey, string pBody);
        IEnumerable<EntityFile> GetFileHeaders(string entity);
        string GetFileData(decimal pKey);
        void SetFileData(decimal pKey, string data);
    }
}