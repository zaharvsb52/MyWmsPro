using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class QlfTest : BaseEntityTest<Qlf>
    {
        public const string ExistsItem1Code = "TST_QLF_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "QLFDESC";
        }

        protected override void FillRequiredFields(Qlf entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.QLFNAME = TestString;
            obj.QLFTYPE = TestString;
        }
    }
}