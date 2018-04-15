using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class VATTypeTest : BaseEntityTest<VATType>
    {
        public const string ExistsItem1Code = "TST_VATTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "VATTYPEINTERESTRATE";
        }

        protected override void FillRequiredFields(VATType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.VATTYPENAME = TestString;
            obj.VATTYPEINTERESTRATE = TestDouble;
        }
    }
}