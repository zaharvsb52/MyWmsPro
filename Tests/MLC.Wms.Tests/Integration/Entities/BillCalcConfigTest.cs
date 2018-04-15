using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillCalcConfigTest : BaseEntityTest<BillCalcConfig>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCCONFIGNAME";
        }

        protected override void FillRequiredFields(BillCalcConfig entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLERCODE_R = BillBillerTest.ExistsItem1Code;
            obj.OPERATION2CONTRACTID_R = BillOperation2ContractTest.ExistsItem1Id;
            obj.CALCDATASOURCECODE_R = PattCalcDataSourceTest.ExistsItem1Code;
            obj.BILLENTITYCODE_R = BillBillEntityTest.ExistsItem1Code;
            obj.CALCCONFIGNAME = TestString;
            obj.CALCCONFIGLOCKED = false;
            obj.CALCCONFIGDATEFROM = DateTime.Now;
            obj.CALCCONFIGDATETILL = DateTime.Now;
        }
    }
}