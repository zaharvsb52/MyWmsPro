using System;
using System.Collections.Generic;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер пользователей.
    /// </summary>
    public class UserManager : WMSBusinessObjectManager<User, string>, IUserManager//, IAuthenticator
    {
        public IEnumerable<User> GetAllFromActiveDirectory()
        {
            using (var repo = GetRepository<IUserRepository>())
                return repo.GetAllFromActiveDirectory();
        }

        [Obsolete]
        public string Authenticate(string login, string credentials)
        {
            using (var repo = GetRepository<IUserRepository>())
                return repo.Authenticate(login, credentials);
        }
        
        public IEnumerable<Right> GetUserRights(string signature)
        {
            using (var repo = GetRepository<IUserRepository>())
                return repo.GetUserRights(signature);
        }
    }
}