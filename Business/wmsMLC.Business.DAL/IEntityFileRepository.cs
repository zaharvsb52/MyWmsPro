using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IEntityFileRepository : IRepository<EntityFile, decimal>
    {
        string GetFileBodyByEntity(string pEntity, string pKey);
        void SetFileBody(string pEntity, string pKey, string pBody);
        string GetFileData(decimal pKey);
        void SetFileData(decimal pKey, string data);
    }
}