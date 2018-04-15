using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillOperationTest : BaseEntityTest<BillOperation>
    {
        public const string ExistsItem1Code = "TST_BILLOPERATION_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillOperation.OperationNamePropertyName;
        }

        protected override void FillRequiredFields(BillOperation entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OPERATIONNAME = TestString;
            obj.OPERATIONCLASSCODE_R = BillOperationClassTest.ExistsItem1Code;
        }
    }
}