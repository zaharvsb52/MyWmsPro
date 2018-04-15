using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture,Ignore("Нужно обсудить с Надеждой фильтрацию отчетов в базе. Сейчас нет прав на новый отчет")]
    public class Report2EntityTest : BaseWMSObjectTest<Report2Entity>
    {
        private readonly ReportTest _reportTest = new ReportTest();

        public Report2EntityTest()
        {
            _reportTest.TestString = TestString;
        }

        protected override void FillRequiredFields(Report2Entity obj)
        {
            base.FillRequiredFields(obj);

            var r = _reportTest.CreateNew();
            var mgr = IoC.Instance.Resolve<ISysObjectManager>();

            obj.AsDynamic().REPORT2ENTITYID = TestDecimal;
            obj.AsDynamic().REPORT2ENTITYREPORT = r.GetKey();
            obj.AsDynamic().REPORT2ENTITYOBJECTNAME = mgr.Get(0);
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(REPORT2ENTITYID = '{0}')", TestDecimal);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _reportTest.ClearForSelf();
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<Report>(TestDecimal, TestString);
        }

    }
}