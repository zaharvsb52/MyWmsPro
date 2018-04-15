using System.Linq;
using BLToolkit.Aspects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public abstract class SecurityChecker : ISecurityChecker
    {
        #region . ISecurityChecker .
        public bool Check(string actionName)
        {
            if (WMSEnvironment.Instance.AuthenticatedUser == null)
                throw new SecurityInsufficientPermissionsException(actionName);

            var signature = WMSEnvironment.Instance.AuthenticatedUser.GetSignature();
            return Check(actionName, signature);
        }

        [Cache]
        public virtual bool Check(string actionName, string userName)
        {
            using (var um = IoC.Instance.Resolve<IUserManager>())
            {
                var allRights = um.GetUserRights(userName);
                //TODO: перевести поиск на RightCode (для этого потребуется найти все места, где используется данный функционал и переделать формат формирования action-а). А потом ВСЕ аккуратно проверить.
                var right = allRights.FirstOrDefault(i => i.RightName.EqIgnoreCase(actionName));
                return right != null;
            }
        }
        #endregion
    }
}
