using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillSpecialFunctionTest : BaseEntityTest<BillSpecialFunction>
    {
        public const string ExistsItem1Code = "TST_BILLSPECIALFUNCTION_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SPECIALFUNCTIONNAME";
        }

        protected override void FillRequiredFields(BillSpecialFunction entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SPECIALFUNCTIONNAME = TestString;
        }
    }
}