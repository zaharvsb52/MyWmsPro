using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillWorkActDetailTest : BaseEntityTest<BillWorkActDetail>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillWorkActDetail.WorkActDetailSum3PropertyName;
        }

        protected override void FillRequiredFields(BillWorkActDetail entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WORKACTID_R = BillWorkActTest.ExistsItem1Id;
            obj.OPERATION2CONTRACTID_R = BillOperation2ContractTest.ExistsItem1Id;
        }
    }
}