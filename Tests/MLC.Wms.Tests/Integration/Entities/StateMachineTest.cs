using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class StateMachineTest : BaseEntityTest<StateMachine>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "NextStatus";
        }
        protected override void FillRequiredFields(StateMachine entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ObjectName_r = SysObjectTest.ExistsItemEntityCode;
            obj.OperationCode_r = BillOperationTest.ExistsItem1Code;
            obj.CurrentStatus = TestString;
            obj.NextStatus = TestString;
        }
    }
}