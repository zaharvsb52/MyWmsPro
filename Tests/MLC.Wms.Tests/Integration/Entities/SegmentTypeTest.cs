using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SegmentTypeTest : BaseEntityTest<SegmentType>
    {
        public const string ExistsItem1Code = "TST_SEGMENTTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SEGMENTTYPEDESC";
        }

        protected override void FillRequiredFields(SegmentType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SegmentTypeName = TestString;
        }
    }
}