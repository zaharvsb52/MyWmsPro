using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PurposeVisitTest : BaseEntityTest<PurposeVisit>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = PurposeVisit.PURPOSEVISITNAMEPropertyName;
        }

        protected override void FillRequiredFields(PurposeVisit entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();

            obj.PURPOSEVISITCODE = TestString;
            obj.PURPOSEVISITNAME = TestString;
        }
    }
}