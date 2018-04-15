using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillCalcEventConfigTest : BaseEntityTest<BillCalcEventConfig>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCEVENTCONFIGDESC";
        }

        protected override void FillRequiredFields(BillCalcEventConfig entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLERCODE_R = BillBillerTest.ExistsItem1Code;
            obj.OPERATION2CONTRACTID_R = BillOperation2ContractTest.ExistsItem1Id;
            obj.CALCDATASOURCECODE_R = PattCalcDataSourceTest.ExistsItem1Code;
            obj.BILLENTITYCODE_R = BillBillEntityTest.ExistsItem1Code;
            obj.CALCEVENTCONFIGNAME = TestString;
            obj.CALCEVENTCONFIGFROM = DateTime.Now;
            obj.CALCEVENTCONFIGFROMTILL = DateTime.Now;
            obj.CALCEVENTCONFIGFIELDDATE = TestString;
            obj.EVENTKINDCODE_R = EventKindTest.ExistsItem1Code;
        }
    }
}