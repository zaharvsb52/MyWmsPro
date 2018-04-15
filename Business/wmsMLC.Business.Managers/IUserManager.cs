using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IUserManager : IBaseManager<User, string>
    {
        IEnumerable<User> GetAllFromActiveDirectory();

        string Authenticate(string login, string credentials);

        IEnumerable<Right> GetUserRights(string signature);
    }
}