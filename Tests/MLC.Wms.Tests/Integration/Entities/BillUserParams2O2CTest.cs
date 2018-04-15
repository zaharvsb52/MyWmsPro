using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillUserParams2O2CTest : BaseEntityTest<BillUserParams2O2C>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "USERPARAMS2O2CAPPLYCODE";
        }

        protected override void FillRequiredFields(BillUserParams2O2C entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLUSERPARAMS2O2CUSERPARAMSCODE = BillUserParamsTest.ExistsItem1Code;
            obj.BILLUSERPARAMS2O2COPERATION2CONTRACTID = BillOperation2ContractTest.ExistsItem1Id;
        }
    }
}