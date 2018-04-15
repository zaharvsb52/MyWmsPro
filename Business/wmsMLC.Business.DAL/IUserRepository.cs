using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IUserRepository : IRepository<User, string>
    {
        IEnumerable<User> GetAllFromActiveDirectory();
        string Authenticate(string login, string credentials);
        IEnumerable<Right> GetUserRights(string signature);
    }
}