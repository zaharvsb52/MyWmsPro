using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MRUseTest : BaseWMSObjectTest<MRUse>
    {
        private readonly MRTest _mrTest = new MRTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] {_mrTest};
        }

        protected override void FillRequiredFields(MRUse obj)
        {
            base.FillRequiredFields(obj);

            var mr = _mrTest.CreateNew();

            obj.AsDynamic().MRUSEID = TestDecimal;
            obj.AsDynamic().MRCODE_R = mr.GetKey();
            obj.AsDynamic().MRUSESTRATEGYTYPE = TestString;
            obj.AsDynamic().MRUSESTRATEGY = TestString;
            obj.AsDynamic().MRUSEORDER = TestDecimal;

        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MRUSEID = '{0}')", TestDecimal);
        }
    }
}