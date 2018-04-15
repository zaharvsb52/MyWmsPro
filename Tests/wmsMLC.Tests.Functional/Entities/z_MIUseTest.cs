using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MIUseTest : BaseWMSObjectTest<MIUse>
    {
        private readonly MITest _miTest = new MITest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _miTest };
        }

        protected override void FillRequiredFields(MIUse obj)
        {
            base.FillRequiredFields(obj);

            var mi = _miTest.CreateNew();

            obj.AsDynamic().MIUSEID = TestDecimal;
            obj.AsDynamic().MICODE_R = mi.GetKey();
            obj.AsDynamic().MIUSESTRATEGYTYPE = TestString;
            obj.AsDynamic().OBJECTENTITYCODE_R = TestString;
            obj.AsDynamic().OBJECTNAME_R = TestString;
            obj.AsDynamic().MIUSEORDER = TestDecimal;
            obj.AsDynamic().MIUSEFILTER = TestString;
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MIUSEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MIUse obj)
        {
            obj.AsDynamic().MIUSESTRATEGYVALUE = TestString;
        }

        protected override void CheckSimpleChange(MIUse source, MIUse dest)
        {
            string sourceName = source.AsDynamic().MIUSESTRATEGYVALUE;
            string destName = dest.AsDynamic().MIUSESTRATEGYVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}