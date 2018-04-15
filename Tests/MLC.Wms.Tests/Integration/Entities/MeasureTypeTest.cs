using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MeasureTypeTest : BaseEntityTest<MeasureType>
    {
        public const string ExistsItem1Code = "TST_MEASURETYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = MeasureType.MeasureTypeNamePropertyName;
        }
    }
}