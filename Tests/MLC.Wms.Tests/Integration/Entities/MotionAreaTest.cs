using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MotionAreaTest : BaseEntityTest<MotionArea>
    {
        public const string ExistsItem1Code = "TST_MOTIONAREA_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MotionAreaDesc";
        }

        protected override void FillRequiredFields(MotionArea entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MotionAreaName = TestString;
        }
    }
}