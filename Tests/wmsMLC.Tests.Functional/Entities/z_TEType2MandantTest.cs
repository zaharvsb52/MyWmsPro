using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TEType2MandantTest : BaseWMSObjectTest<TEType2Mandant>
    {
        private readonly TETypeTest _teTypeTest = new TETypeTest();
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _teTypeTest, _mandantTest };
        }

        protected override void FillRequiredFields(TEType2Mandant obj)
        {
            base.FillRequiredFields(obj);

            var teType = _teTypeTest.CreateNew();
            var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().TETYPE2MANDANTID = TestDecimal;
            obj.AsDynamic().TETYPE2MANDANTTETYPECODE = teType.GetKey();
            obj.AsDynamic().MANDANTID = mandant.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TETYPE2MANDANTID = '{0}')", TestDecimal);
        }
    }
}