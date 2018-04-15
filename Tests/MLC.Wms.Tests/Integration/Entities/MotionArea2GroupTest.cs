using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MotionArea2GroupTest : BaseEntityTest<MotionArea2Group>
    {
        protected override void FillRequiredFields(MotionArea2Group entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MOTIONAREA2GROUPMOTIONAREAGROUPCODE = MotionAreaGroupTest.ExistsItem1Code;
            obj.MOTIONAREA2GROUPMOTIONAREACODE = MotionAreaTest.ExistsItem1Code;
        }
    }
}