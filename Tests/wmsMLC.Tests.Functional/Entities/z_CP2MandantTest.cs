using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CP2MandantTest : BaseWMSObjectTest<CP2Mandant>
    {
        private readonly MandantTest _mandantTest = new MandantTest();
        private readonly CustomParamTest _customParamTest = new CustomParamTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest, _customParamTest };
        }

        protected override void FillRequiredFields(CP2Mandant obj)
        {
            base.FillRequiredFields(obj);

            var m = _mandantTest.CreateNew();
            var c = _customParamTest.CreateNew();

            obj.AsDynamic().CP2MANDANTID = TestDecimal;
            obj.AsDynamic().MANDANTID = m.GetKey();
            obj.AsDynamic().CP2MANDANTCUSTOMPARAMCODE = c.GetKey();
            obj.AsDynamic().CP2MANDANTMUSTSET = false;
            obj.AsDynamic().CP2MANDANTORDER = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CP2MANDANTID = '{0}')", TestDecimal);
        }
    }
}