using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvTaskStepTest : BaseWMSObjectTest<InvTaskStep>
    {
        private readonly InvTaskGroupTest _invTaskGroupTest = new InvTaskGroupTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invTaskGroupTest };
        }

        protected override void FillRequiredFields(InvTaskStep obj)
        {
            base.FillRequiredFields(obj);

            var invTaskGroup = _invTaskGroupTest.CreateNew();

            obj.AsDynamic().INVTASKSTEPID = TestDecimal;
            obj.AsDynamic().INVTASKGROUPID_R = invTaskGroup.GetKey();
            obj.AsDynamic().INVTASKSTEPNUMBER = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVTASKSTEPID = '{0}')", TestDecimal);
        }
    }
}
