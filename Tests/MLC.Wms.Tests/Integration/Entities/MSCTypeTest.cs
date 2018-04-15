using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MSCTypeTest : BaseEntityTest<MSCType>
    {
        public const string ExistsItem1Code = "TST_MSCTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MSCTypeDesc";
        }

        protected override void FillRequiredFields(MSCType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MSCTypeName = TestString;
            obj.MSCTypeOrder = TestDecimal;
        }
    }
}