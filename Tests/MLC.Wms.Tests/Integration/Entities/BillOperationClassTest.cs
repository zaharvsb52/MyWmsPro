using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillOperationClassTest : BaseEntityTest<BillOperationClass>
    {
        public const string ExistsItem1Code = "TST_BILLOPERATIONCLASS_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillOperationClass.OperationClassNamePropertyName;
        }

        protected override void FillRequiredFields(BillOperationClass entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OPERATIONCLASSNAME = TestString;
        }
    }
}