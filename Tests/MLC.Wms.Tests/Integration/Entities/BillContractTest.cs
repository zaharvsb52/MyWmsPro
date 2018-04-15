using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillContractTest : BaseEntityTest<BillContract>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillContract.CONTRACTDESCPropertyName;
        }

        protected override void FillRequiredFields(BillContract entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CONTRACTDATEFROM = DateTime.Now;
            obj.CONTRACTOWNER = TstMandantId;
            obj.CONTRACTCUSTOMER = TstMandantId;
            obj.CURRENCYCODE_R = IsoCurrencyTest.ExistsItem1Code;
            obj.VATTYPECODE_R = VATTypeTest.ExistsItem1Code;
        }
    }
}