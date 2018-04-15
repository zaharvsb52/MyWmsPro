using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillCalcVerificationTest : BaseEntityTest<BillCalcVerification>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCVERIFICATIONDESC";
        }

        protected override void FillRequiredFields(BillCalcVerification entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CALCVERIFICATIONID = TestDecimal;
            obj.BILLERCODE_R = BillBillerTest.ExistsItem1Code;
            obj.OPERATION2CONTRACTID_R = BillOperation2ContractTest.ExistsItem1Id;
            obj.CALCDATASOURCECODE_R = PattCalcDataSourceTest.ExistsItem1Code;
            obj.BILLENTITYCODE_R = BillBillEntityTest.ExistsItem1Code;
            obj.CALCVERIFICATIONNAME = TestString;
            obj.CALCVERIFICATIONFROM = DateTime.Now;
            obj.CALCVERIFICATIONTILL = DateTime.Now;
            obj.CALCVERIFICATIONMESSAGE = TestString;
            obj.CALCVERIFICATIONFIELDEXCEPTION = TestString;
        }
    }
}