using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ReportFileTest : BaseWMSObjectTest<ReportFile>
    {
        [Test, Ignore()]
        public void Test()
        {
            var mgr = new ReportFileManager();
            var filename = "parameter1.frx";
            var blob = mgr.GetReportFileBody(filename);
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(REPORTFILE = '{0}')", TestString);
        }

        protected override void FillRequiredFields(ReportFile obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().REPORTFILEFILE = TestString;
        }

        protected override void MakeSimpleChange(ReportFile obj)
        {
            obj.AsDynamic().REPORTFILENAME = TestString;
        }

        protected override void CheckSimpleChange(ReportFile source, ReportFile dest)
        {
            string sourceName = source.AsDynamic().REPORTFILENAME;
            string destName = dest.AsDynamic().REPORTFILENAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Зацикливание теста")]
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
        [Test,Ignore("Ошибка фильтра в БД по запросу select * from TABLE(PKGREPORTFILE.getReportFileLst(null,'REPORTFILEID is null AND REPORTFILE is null AND REPORTFILEHASHCODE is null AND REPORTFILESUBFOLDER is null AND REPORTFILELOCKED is null AND REPORTFILENAME is null AND REPORTFILEDESC is null AND USERINS is null AND DATEINS is null AND USERUPD is null AND DATEUPD is null AND REPORTFILEBODY is null AND TRANSACT is null'))")]
        public override void FiltersTest()
        {
            
        }
    }
}