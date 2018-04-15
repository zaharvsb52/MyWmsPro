using System.Collections.Generic;
using System.Data;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Oracle;

namespace wmsMLC.Business.DAL.Oracle
{
//    public abstract class EpsOutputRepository : Repository<EpsOutput, decimal>, IEpsOutputRepository
//    {
//        //private const string InsertEpsOutputFunctionName = "pkgEpsOutput.insEpsOutputTemp";
//
//        //public void Insert(string pEpsHandler, string pReportFile, string pPhysicalPrinter, string pCopies, string pReportParam)
//        //{
//        //    var result = RunManualDbOperation(db =>
//        //    {
//        //        var ps = db.GetSpParameters(InsertEpsOutputFunctionName, false, false);
//        //        var lst = new List<IDbDataParameter>();
//        //        //var p = new OracleParameter("pEpsHandler", ConvertToDecimal(db, pEpsHandler, null));
//        //        //lst.Add(p);
//        //        ps[0].Value = pReportFile;
//        //        ps[1].Value = pPhysicalPrinter;
//        //        ps[2].Value = ConvertToDecimal(db, pCopies, null);
//        //        ps[3].Value = pReportParam;
//        //        lst.AddRange(ps);
//
//        //        var stm = string.Format("call {0}({1}, :pReportFile, :pPhysicalPrinter, :pCopies, :pReportParam)", InsertEpsOutputFunctionName, ConvertToDecimal(db, pEpsHandler, null));
//        //        //var stm = string.Format("call {0}(:pEpsHandler, :pReportFile, :pPhysicalPrinter, :pCopies, :pReportParam)", InsertEpsOutputFunctionName);
//        //        return db.SetCommand(stm, lst.ToArray()).ExecuteScalar<EpsOutput>();
//        //    });
//        //}
//
//        private const string InsertEpsOutputFunctionName = " pkgEpsOutput.insEpsOutputTemp";
//
//        public void Insert(string pReportFile, string pResultReportFile, string pFileFormat, string pReportParam1, string pReportParam2, string pReportValue1, string pReportValue2)
//        {
//            var result = RunManualDbOperation(db =>
//            {
//                var ps = db.GetSpParameters(InsertEpsOutputFunctionName, false, false);
//                var lst = new List<IDbDataParameter>();
//                ps[0].Value = pResultReportFile;
//                ps[1].Value = pFileFormat;
//                ps[2].Value = pReportParam1;
//                ps[3].Value = pReportParam2;
//                ps[4].Value = pReportValue1;
//                ps[5].Value = pReportValue2;
//                lst.AddRange(ps);
//
//                var stm = string.Format("call {0}('{1}', :pResultReportFile, :pFileFormat, :pReportParam1, :pReportParam2, :pReportValue1, :pReportValue2)", InsertEpsOutputFunctionName, pReportFile);
//                return db.SetCommand(stm, lst.ToArray()).ExecuteScalar<EpsOutput>();
//            });
//        }
//    }
}
