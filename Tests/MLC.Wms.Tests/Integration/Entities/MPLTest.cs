using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MPLTest : BaseEntityTest<MPL>
    {
        public const string ExistsItem1Code = "TST_MPL_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MPLDesc";
        }

        protected override void FillRequiredFields(MPL entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MPLName = TestString;
        }
    }
}