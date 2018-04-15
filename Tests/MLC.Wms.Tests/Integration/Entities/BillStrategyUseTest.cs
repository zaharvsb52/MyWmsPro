using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillStrategyUseTest : BaseEntityTest<BillStrategyUse>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "STRATEGYUSENAME";
        }

        protected override void FillRequiredFields(BillStrategyUse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.STRATEGYCODE_R = BillStrategyTest.ExistsItem1Code;
            obj.OPERATION2CONTRACTID_R = BillOperation2ContractTest.ExistsItem1Id;
            obj.STRATEGYUSENAME = TestString;
            obj.STRATEGYUSEORDER = TestDecimal;
            obj.STRATEGYUSEFROM = DateTime.Now;
            obj.STRATEGYUSETILL = DateTime.Now;
        }
    }
}