using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillScale2O2CTest : BaseEntityTest<BillScale2O2C>
    {
        protected override void FillRequiredFields(BillScale2O2C entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLSCALE2O2CSCALECODE = BillScaleTest.ExistsItem1Code;
            obj.BILLSCALE2O2COPERATION2CONTRACTID = BillOperation2ContractTest.ExistsItem1Id;
        }
    }
}