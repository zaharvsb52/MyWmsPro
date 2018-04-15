using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvTaskGroupTest : BaseWMSObjectTest<InvTaskGroup>
    {
        private readonly InvGroupTest _invGroupTest = new InvGroupTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invGroupTest };
        }

        protected override void FillRequiredFields(InvTaskGroup obj)
        {
            base.FillRequiredFields(obj);

            var invGroup = _invGroupTest.CreateNew();

            obj.AsDynamic().INVTASKGROUPID = TestDecimal;
            obj.AsDynamic().INVGROUPID_R = invGroup.GetKey();
            obj.AsDynamic().INVTASKGROUPNUMBER = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVTASKGROUPID = '{0}')", TestDecimal);
        }
    }
}