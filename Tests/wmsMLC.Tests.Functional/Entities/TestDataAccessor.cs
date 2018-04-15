using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using BLToolkit.Data;
using BLToolkit.DataAccess;
using wmsMLC.General.DAL.Oracle;

namespace wmsMLC.Tests.Functional.Entities
{
    public abstract class TestDataAccessor : DataAccessor
    {
        [SprocName("procTestXML")]
        public abstract void procTestXMLInternal(out XmlDocument[] paramXml);

        [SprocName("pkgsysobject.getSysObjectLst2")]
        [ScalarSource(ScalarSourceType.ReturnValue)]
        public abstract string[] GetSysObjects(XmlDocument attrentity, string filter);

        public IEnumerable<XmlDocument> GetSysObjectXml()
        {
            using (var db = GetDbManager())
            {
                var stm = string.Format("select SYS.XMLTYPE.GETSTRINGVAL(COLUMN_VALUE) from TABLE(pkgsysobject.getSysObjectLst(null, null))");
                var startTime = DateTime.Now;
                var objects = new List<string>();
                var cmd = db.SetCommand(stm);
                using (var reader = cmd.ExecuteReader())
                {
                    //var specReader = (IOracleDataReader)reader;
                    while (reader.Read())
                    {
                        var obj = reader.GetString(0);
                        objects.Add(obj);
                    }
                }
                var dif = DateTime.Now - startTime;
                Console.WriteLine(dif);
                var items = objects.AsParallel().Select(i => { var doc = new XmlDocument(); doc.LoadXml(i); return doc; }).ToArray();
                dif = DateTime.Now - startTime;
                Console.WriteLine(dif);
                return null;
            }
        }
    }
}