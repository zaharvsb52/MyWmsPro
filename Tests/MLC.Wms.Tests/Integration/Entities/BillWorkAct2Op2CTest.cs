using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillWorkAct2Op2CTest : BaseEntityTest<BillWorkAct2Op2C>
    {
        protected override void FillRequiredFields(BillWorkAct2Op2C entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLWORKACT2OP2CWORKACTID = BillWorkActTest.ExistsItem1Id;
            obj.BILLWORKACT2OP2COPERATION2CONTRACTID = BillOperation2ContractTest.ExistsItem1Id;
        }
    }
}