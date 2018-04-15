using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class QResTest : BaseWMSObjectTest<QRes>
    {
        //private readonly MandantTest _mandantTest = new MandantTest();
        private readonly OWBTest _owbTest = new OWBTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { /*_mandantTest,*/ _owbTest };
        }

        protected override void FillRequiredFields(QRes obj)
        {
            base.FillRequiredFields(obj);

            //var mandant = _mandantTest.CreateNew();
            var owb = _owbTest.CreateNew();
            
            obj.AsDynamic().QRESID = TestDecimal;
            obj.AsDynamic().MANDANTID = 1;//mandant.GetKey();
            obj.AsDynamic().OWBID_R = owb.GetKey();
            obj.AsDynamic().OWBPRIORITY_R = TestDecimal;
            obj.AsDynamic().OWBPRODUCTNEED_R = TestString;
            obj.AsDynamic().OWBRESERVTYPE_R = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(QRESID = '{0}')", TestDecimal);
        }
    }
}