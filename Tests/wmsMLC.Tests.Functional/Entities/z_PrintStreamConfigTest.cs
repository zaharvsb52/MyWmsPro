using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PrintStreamConfigTest : BaseWMSObjectTest<PrintStreamConfig>
    {
        [Test, Ignore("Для отладки процедуры")]
        public void TestGetDefaultPrinter()
        {
            var mng = IoC.Instance.Resolve<IBaseManager<Report2Entity>>() as IReport2EntityManager;
            PrintStreamConfig result = null;

            //var result = mng.GetDefaultPrinter(reportfilename: "TestOlga", host: "Host_r", login: "Login_r", mandantcode: 1111);
            //var result = mng.GetDefaultPrinter(reportfilename: "Test002frx", host: "Test B52s", login: "TU95MS", mandantcode: null);

            //var result = mng.GetDefaultPrinter(reportfilename: "TestOlga", host: "Host", login: null, mandantcode: null); //no
            //var result = mng.GetDefaultPrinter(reportfilename: "TestOlga", host: "Host", login: null, mandantcode: "1111"); //no


            result = mng.GetDefaultPrinter(reportfilename: "Test002frx", host: "P8Z77-002", login: null, mandantcode: null); //no

            result = mng.GetDefaultPrinter(reportfilename: "TestOlga", host: "Host", login: null, mandantcode: 1111); //yes
            result = mng.GetDefaultPrinter(reportfilename: "TestOlga", host: "Host", login: "Marysheva_OV", mandantcode: 1111); //yes
            result = mng.GetDefaultPrinter(reportfilename: "Паучатинка", host: null, login: null, mandantcode: null); //yes

            var result2 = mng.GetDefaultReport(entity: new Mandant(), host: null, reportfilename: "TestOlga", mandantcode: null); //yes
            result2 = mng.GetDefaultReport(entity: new Mandant(), host: "P8Z77 - 006", reportfilename: "Паучатинка", mandantcode: 4); //no
        }

        public const bool TestBool = true;
        private readonly ReportTest _reportTest = new ReportTest();
        private readonly PrinterLogicalTest _printerLogicalTest = new PrinterLogicalTest();

        protected override string GetCheckFilter()
        {
            return string.Format("(upper(HOST_R) like upper('{0}%'))", AutoTestMagicWord);
        }

        protected override void FillRequiredFields(PrintStreamConfig obj)
        {
            base.FillRequiredFields(obj);

            var report = _reportTest.CreateNew();
            var printerLogical = _printerLogicalTest.CreateNew();

            obj.AsDynamic().HOST_R = TestString;
            obj.AsDynamic().LOGIN_R = "TECH_AUTOTEST";
            obj.AsDynamic().REPORT_R = report.GetKey();
            obj.AsDynamic().LOGICALPRINTER_R = printerLogical.GetKey();
            obj.AsDynamic().PRINTSTREAMCOPIES = TestDecimal;
            obj.AsDynamic().PRINTSTREAMLOCKED = 1;
        }

        protected override void MakeSimpleChange(PrintStreamConfig obj)
        {
            obj.AsDynamic().PRINTSTREAMDESC = TestString;
        }

        protected override void CheckSimpleChange(PrintStreamConfig source, PrintStreamConfig dest)
        {
            string sourceName = source.AsDynamic().PRINTSTREAMDESC;
            string destName = dest.AsDynamic().PRINTSTREAMDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test,Ignore("Зацикливание теста")]
        public override void ManagerGetFilteredTest()
        {
            
        }
        [Test, Ignore("Зацикливание теста")]
        public override void ManagerCRUDTest()
        {
            
        }
        [Test, Ignore("Зацикливание теста")]
        public override void ManagerGetAllTest()
        {
            
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _reportTest.ClearForSelf();
            _printerLogicalTest.ClearForSelf();
        }
    }
}
