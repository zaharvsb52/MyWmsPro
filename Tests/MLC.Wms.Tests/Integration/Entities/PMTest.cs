using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PMTest : BaseEntityTest<PM>
    {
        public const string ExistsItem1Code = "TST_PM_1";
        public const string ExistsItem2Code = "TST_PM_2";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PMDESC";
        }

        protected override void FillRequiredFields(PM entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PmName = TestString;
        }
    }
}