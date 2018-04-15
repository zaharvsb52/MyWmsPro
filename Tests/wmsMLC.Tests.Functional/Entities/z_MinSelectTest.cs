using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MinSelectTest : BaseWMSObjectTest<MinSelect>
    {
        private readonly MinTest _minTest = new MinTest();
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _minTest, _mandantTest };
        }

        protected override void FillRequiredFields(MinSelect obj)
        {
            base.FillRequiredFields(obj);

            _mandantTest.TestDecimal = TestDecimal + 1;
            _mandantTest.TestString = TestString + "1";

            obj.AsDynamic().MINSELECTID = TestDecimal;
            obj.AsDynamic().MINID_R = _minTest.CreateNew().GetKey(); ;
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().MANDANTID = _mandantTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MINSELECTID = '{0}')", TestDecimal);
        }
    }
}