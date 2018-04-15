using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MotionAreaGroupTest : BaseEntityTest<MotionAreaGroup>
    {
        public const string ExistsItem1Code = "TST_MOTIONAREAGROUP_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MotionAreaGroupDesc";
        }

        protected override void FillRequiredFields(MotionAreaGroup entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MotionAreaGroupName = TestString;
        }
    }
}