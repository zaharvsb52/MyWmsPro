using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PMMethodTest : BaseEntityTest<PMMethod>
    {
        public const string ExistsItem1Code = "TST_PMMETHOD_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PMMETHODDESC";
        }

        protected override void FillRequiredFields(PMMethod entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PMMETHODNAME = TestString;
        }
    }
}