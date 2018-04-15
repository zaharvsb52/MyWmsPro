using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PMMethod2OperationTest : BaseEntityTest<PMMethod2Operation>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PMMETHOD2OPERATIONDESC";
        }

        protected override void FillRequiredFields(PMMethod2Operation entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PMMETHOD2OPERATIONOPERATIONCODE = BillOperationTest.ExistsItem1Code;
            obj.PMMETHOD2OPERATIONPMMETHODCODE = PMMethodTest.ExistsItem1Code;
            obj.PMMETHOD2OPERATIONCONFIG2OBJECTID = Config2ObjectTest.ExistsItem1Id;
        }
    }
}