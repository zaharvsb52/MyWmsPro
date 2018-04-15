using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MRTest : BaseEntityTest<MR>
    {
        public const string ExistsItem1Code = "TST_MR_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MRDesc";
        }

        protected override void FillRequiredFields(MR entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MRName = TestString;
        }
    }
}