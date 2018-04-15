using System.Collections.Generic;
using System.Data;
using System.Xml;
using Oracle.DataAccess.Client;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class CustomParamRepository : BaseHistoryRepository<CustomParam, string>, ICustomParamRepository
    {
        private const string PkgName = "pkgCustomParam";

        public IEnumerable<CustomParam> GetCPByInstance(string entity, string key, string attrentity, string cpSource, string cpTarget)
        {
            const string pkg = PkgName + ".getCPByInstance";
            return RunManualDbOperation(db =>
            {
                var ps = db.GetSpParameters(pkg, false, false);
                ps[0].Value = entity;
                ps[1].Value = key;
                ps[2].Value = attrentity;
                ps[3].Value = cpSource;
                ps[4].Value = cpTarget;
                // INFO: не знаю почему, но так не работает
                //var ps = new[]
                //{
                //    new OracleParameter("PENTITY", OracleDbType.Varchar2, entity, ParameterDirection.Input){DbType = DbType.String},
                //    new OracleParameter("PKEY ", OracleDbType.Varchar2, key, ParameterDirection.Input){DbType = DbType.String},
                //    new OracleParameter("PATTRENTITY", OracleDbType.XmlType, null, ParameterDirection.Input){DbType = DbType.String}
                //};

                var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE({0}(:pEntity, :pKey, :pAttrEntity, :pCPSource, :pCPTarget))", pkg);
                var resXml = db.SetCommand(stm, ps).ExecuteScalarList<XmlDocument>();                
                return XmlDocumentConverter.ConvertToListOf<CustomParam>(resXml);
            });
        }
    }
}