using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillTariffTest : BaseEntityTest<BillTariff>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillTariff.BILLTariffValuePropertyName;
        }

        protected override void FillRequiredFields(BillTariff entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OPERATION2CONTRACTID_R = BillOperation2ContractTest.ExistsItem1Id;
            obj.TARIFFDATEFROM = DateTime.Now;
            obj.TARIFFDATETILL = DateTime.Now;
            obj.TARIFFVALUE = TestDouble;
        }
    }
}