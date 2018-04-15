using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Truck2MotionAreaGroupTest : BaseEntityTest<Truck2MotionAreaGroup>
    {
        protected override void FillRequiredFields(Truck2MotionAreaGroup entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TRUCK2MOTIONAREAGROUPTRUCKCODE = TruckTest.ExistsItem1Code;
            obj.TRUCK2MOTIONAREAGROUPMOTIONAREAGROUPCODE = MotionAreaGroupTest.ExistsItem1Code;
        }
    }
}