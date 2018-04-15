using System.Data;
using System.Text;
using Oracle.DataAccess.Client;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class XamlRepository<T, TKey> : BaseHistoryRepository<T, TKey>, IXamlRepository<T, TKey> where T : class, new()
    {
        protected string PkgName;
        protected string GetName;
        protected string UpdName;

        public virtual string GetXaml(TKey pKey)
        {
            string pkg = string.Format("{0}.{1}", PkgName, GetName);
            var result = RunManualDbOperation(db =>
            {
                var p1 = new OracleParameter("pKey", OracleDbType.Varchar2, pKey, ParameterDirection.Input);
                var stm = string.Format("select {0}(:{1}) from dual", pkg, p1.ParameterName);
                return db.SetCommand(stm, p1).ExecuteScalar<byte[]>();
            });
            return result != null ? Encoding.UTF8.GetString(result) : string.Empty;
        }

        public virtual void SetXaml(TKey pKey, string xaml)
        {
            string pkg = string.Format("{0}.{1}", PkgName, UpdName);
            var result = RunManualDbOperation(db =>
            {
                var ps = db.GetSpParameters(pkg, true, false);
                ps[0].Value = pKey;
                ps[1].Value = Encoding.UTF8.GetBytes(xaml);

                var stm = string.Format("call {0}(:{1},:{2})", pkg, ps[0].ParameterName, ps[1].ParameterName);
                return db.SetCommand(stm, ps).ExecuteScalar();
            });
        }
    }
}