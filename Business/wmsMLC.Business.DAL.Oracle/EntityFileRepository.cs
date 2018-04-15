using System.Data;
using System.Text;
using BLToolkit.DataAccess;
using Oracle.DataAccess.Client;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class EntityFileRepository : BaseHistoryCacheableRepository<EntityFile, decimal>, IEntityFileRepository
    {
        private const string PkgName = "pkgEntityFile";
        private const string GetFileBodyByEntityFunctionName = PkgName + ".getFileBodyByEntity";

        public string GetFileBodyByEntity(string pEntity, string pKey)
        {
            var result = RunManualDbOperation(db =>
                {
                    var ps = db.GetSpParameters(GetFileBodyByEntityFunctionName, false, false);
                    ps[0].Value = pEntity;
                    ps[1].Value = pKey;
                    var stm = string.Format("select {0}(:pentity, :pkey) from dual", GetFileBodyByEntityFunctionName);
                    return db.SetCommand(stm, ps).ExecuteScalar<byte[]>();
                });
            return result == null ? null : Encoding.UTF8.GetString(result);
        }

        [SprocName(PkgName + ".setFileBody")]
        [DiscoverParameters(false)]
        public abstract void SetFileBodyInternal(string entity, string key, byte[] body);
        public void SetFileBody(string pEntity, string pKey, string pBody)
        {
            SetFileBodyInternal(pEntity, pKey, Encoding.UTF8.GetBytes(pBody));
        }

        public string GetFileData(decimal pKey)
        {
            var pkg = string.Format("{0}.{1}", PkgName, "getEntityFileFileData");
            var result = RunManualDbOperation(db =>
            {
                var p1 = new OracleParameter("pKey", OracleDbType.Varchar2, pKey, ParameterDirection.Input);
                var stm = string.Format("select {0}(:{1}) from dual", pkg, p1.ParameterName);
                return db.SetCommand(stm, p1).ExecuteScalar<byte[]>();
            });
            return result != null ? System.Convert.ToBase64String(result) : string.Empty;
        }

        public void SetFileData(decimal pKey, string data)
        {
            var pkg = string.Format("{0}.{1}", PkgName, "updEntityFileFileData");
            RunManualDbOperation(db =>
            {
                var ps = db.GetSpParameters(pkg, true, false);
                ps[0].Value = pKey;
                ps[1].Value = System.Convert.FromBase64String(data);

                var stm = string.Format("call {0}(:{1},:{2})", pkg, ps[0].ParameterName, ps[1].ParameterName);
                return db.SetCommand(stm, ps).ExecuteScalar();
            });
        }
    }
}