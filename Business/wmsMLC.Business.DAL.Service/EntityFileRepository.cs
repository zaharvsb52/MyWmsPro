using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class EntityFileRepository : BaseRepository<EntityFile, decimal>, IEntityFileRepository 
    {
        public string GetFileBodyByEntity(string pEntity, string pKey)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var entityParam = new TransmitterParam { Name = "pEntity", Type = typeof(string), IsOut = false, Value = pEntity };
            var keyParam = new TransmitterParam { Name = "pKey", Type = typeof(string), IsOut = false, Value = pKey };
            var telegram = new RepoQueryTelegramWrapper(typeof(EntityFile).Name, "GetFileBodyByEntity", new[] { resultParam, entityParam, keyParam });
            ProcessTelegramm(telegram);
            return (string)resultParam.Value;
        }

        public void SetFileBody(string pEntity, string pKey, string pBody)
        {
            var entityParam = new TransmitterParam { Name = "pEntity", Type = typeof(string), IsOut = false, Value = pEntity };
            var keyParam = new TransmitterParam { Name = "pKey", Type = typeof(string), IsOut = false, Value = pKey };
            var bodyParam = new TransmitterParam { Name = "pBody", Type = typeof(string), IsOut = false, Value = pBody };
            var telegram = new RepoQueryTelegramWrapper(typeof(EntityFile).Name, "SetFileBody", new[] { entityParam, keyParam, bodyParam });
            ProcessTelegramm(telegram);
        }

        public string GetFileData(decimal pKey)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var pKeyParam = new TransmitterParam { Name = "pKey", Type = typeof(decimal), IsOut = false, Value = pKey };
            var telegram = new RepoQueryTelegramWrapper(typeof(EntityFile).Name, "GetFileData", new[] { resultParam, pKeyParam });
            ProcessTelegramm(telegram);
            return resultParam.Value == null ? null : resultParam.Value.ToString();
        }

        public void SetFileData(decimal pKey, string data)
        {
            var pKeyParam = new TransmitterParam { Name = "pKey", Type = typeof(decimal), IsOut = false, Value = pKey };
            var dataParam = new TransmitterParam { Name = "data", Type = typeof(string), IsOut = false, Value = data };
            var telegram = new RepoQueryTelegramWrapper(typeof(EntityFile).Name, "SetFileData", new[] { pKeyParam, dataParam });
            ProcessTelegramm(telegram);
        }
    }
}