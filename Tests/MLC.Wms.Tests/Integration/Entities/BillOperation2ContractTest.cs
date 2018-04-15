using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillOperation2ContractTest : BaseEntityTest<BillOperation2Contract>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillOperation2Contract.OPERATION2CONTRACTDESCPropertyName;
        }

        protected override void FillRequiredFields(BillOperation2Contract entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLOPERATION2CONTRACTCONTRACTID = BillContractTest.ExistsItem1Id;
            obj.OPERATION2CONTRACTNAME = TestString;
            obj.BILLOPERATION2CONTRACTANALYTICSCODE = BillAnalyticsTest.ExistsItem1Code;
            obj.OPERATION2CONTRACTCODE = TestString;
        }
    }
}                