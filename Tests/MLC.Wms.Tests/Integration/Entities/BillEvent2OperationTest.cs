using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillEvent2OperationTest : BaseEntityTest<BillEvent2Operation>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PRIORITY";
        }

        protected override void FillRequiredFields(BillEvent2Operation entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLEVENT2OPERATIONEVENTKINDCODE = EventKindTest.ExistsItem1Code;
            obj.BILLEVENT2OPERATIONOPERATIONCODE = BillOperationTest.ExistsItem1Code;
            obj.PRIORITY = TestDecimal;
            obj.EVENT2OPERATIONBUSINESS = "UNKNOWN";
        }        
    }
}