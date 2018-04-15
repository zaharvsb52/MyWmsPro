using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvGroupTest : BaseWMSObjectTest<InvGroup>
    {
        private readonly InvTest _invTest = new InvTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invTest };
        }
        
        protected override void FillRequiredFields(InvGroup obj)
        {
            base.FillRequiredFields(obj);

            _invTest.TestGuid = TestGuid;
            var inv = _invTest.CreateNew();

            obj.AsDynamic().INVGROUPID = TestDecimal;
            obj.AsDynamic().INVID_R = inv.GetKey();
            obj.AsDynamic().INVGROUPNAME = TestString;
            obj.AsDynamic().INVGROUPFILTER = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVGROUPID = '{0}')", TestDecimal);
        }
    }
}