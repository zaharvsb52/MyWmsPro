using System.Collections.Generic;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    /// <summary>
    /// Репозиторий Пользователей
    /// </summary>
    public abstract class UserRepository : BaseHistoryRepository<User, string>, IUserRepository
    {
        public virtual IEnumerable<User> GetAllFromActiveDirectory()
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<User>), IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(User).Name, "GetAllFromActiveDirectory", new[] { resultParam });
            ProcessTelegramm(telegram);
            return (List<User>)resultParam.Value;
        }

        public string Authenticate(string login, string credentials)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(string), IsOut = true };
            var loginParam = new TransmitterParam { Name = "login", Type = typeof(string), IsOut = false, Value = login};
            var credentialsParam = new TransmitterParam { Name = "credentials", Type = typeof(string), IsOut = false, Value = credentials };
            var telegram = new RepoQueryTelegramWrapper(typeof(User).Name, "Authenticate", new[] { resultParam, loginParam, credentialsParam });
            ProcessTelegramm(telegram);
            return (string)resultParam.Value;
        }

        [Cache]
        public virtual IEnumerable<Right> GetUserRights(string signature)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<Right>), IsOut = true };
            var signatureParam = new TransmitterParam { Name = "signature", Type = typeof(string), IsOut = false, Value = signature };
            var telegram = new RepoQueryTelegramWrapper(typeof(User).Name, "GetUserRights", new[] { resultParam, signatureParam });
            ProcessTelegramm(telegram);
            return (List<Right>)resultParam.Value;
        }
    }
}