using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class IsoCurrencyTest : BaseEntityTest<IsoCurrency>
    {
        public const string ExistsItem1Code = "-1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CURRENCYNAMEENG";
        }

        protected override void FillRequiredFields(IsoCurrency entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CURRENCYCODE = TestString.Substring(0, 3);
            obj.CURRENCYNUMERIC = TestString.Substring(0, 3);
        }
    }
}