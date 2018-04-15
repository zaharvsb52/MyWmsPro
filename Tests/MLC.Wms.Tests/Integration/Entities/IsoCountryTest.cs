using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class IsoCountryTest : BaseEntityTest<IsoCountry>
    {
        public const string ExistsItem1Code = "TST";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            TestString = "-IC";
            InsertItemStringId = TestString;
            SimpleChangePropertyName = "COUNTRYNAMEENG";
        }

        protected override void FillRequiredFields(IsoCountry entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CountryAlpha2 = TestString.Substring(0, 2);
            obj.CountryNumeric = TestString;
        }
    }
}