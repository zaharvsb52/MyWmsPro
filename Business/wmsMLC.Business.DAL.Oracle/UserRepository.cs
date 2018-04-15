using System.Collections.Generic;
using System.Xml;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class UserRepository : BaseHistoryRepository<User, string>, IUserRepository
    {
        public IEnumerable<User> GetAllFromActiveDirectory()
        {
            return RunManualDbOperation(db =>
            {
                var sql = "select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE(pkgUser.GetFromADUserLst(NULL, NULL))";
                var resXml = db.SetCommand(sql).ExecuteScalarList<XmlDocument>();
                return XmlDocumentConverter.ConvertToListOf<User>(resXml);
            });
        }

        public string Authenticate(string login, string credentials)
        {
            return RunManualDbOperation(db =>
            {
                var pLogin = db.InputParameter("pLogin", login);
                var pCred = db.InputParameter("pCredentials", credentials);

                //                    var ps = new IDbDataParameter[]
                //                    {
                //                        new OracleParameter("plogin", OracleDbType.Varchar2, login, ParameterDirection.Input),
                //                        new OracleParameter("pcredentials", OracleDbType.Varchar2, credentials, ParameterDirection.Input)
                //                    };

                var stm = "select pkgUser.Authenticate(:plogin, :pcredentials) from dual";
                return db.SetCommand(stm, pLogin, pCred).ExecuteScalar<string>();
            });
        }

        public virtual IEnumerable<Right> GetUserRights(string signature)
        {
            return RunManualDbOperation(db =>
            {
                var ps = db.InputParameter("pKey", signature);
                var sql = "select rright.rightcode, rright.rightname from vwUserRights inner join rright on rright.rightcode = vwUserRights.rightcode_r where vwUserRights.usercode_r = :pKey and rright.rightlocked = 0";
                return db.SetCommand(sql, ps).ExecuteList<Right>();
            });
        }
    }
}