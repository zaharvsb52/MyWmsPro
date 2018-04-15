using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MSCTest : BaseEntityTest<MSC>
    {
        public const string ExistsItem1Code = "TST_MSC_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MSCDesc";
        }

        protected override void FillRequiredFields(MSC entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MSCName = TestString;
            obj.MSCTypeCode_r = MSCTypeTest.ExistsItem1Code;
            obj.MSCTargetSupplyArea = TestString;
            obj.MSCOperationOrder = TestDecimal;
            obj.MSCFinal = TestBool;
        }
    }
}