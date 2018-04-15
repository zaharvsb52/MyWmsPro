using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Truck2MotionAreaGroupTest : BaseWMSObjectTest<Truck2MotionAreaGroup>
    {
        private readonly TruckTest _truckTest = new TruckTest();
        private readonly MotionAreaGroupTest _motionAreaGroupTest = new MotionAreaGroupTest();

        protected override void FillRequiredFields(Truck2MotionAreaGroup obj)
        {
            base.FillRequiredFields(obj);

            var t = _truckTest.CreateNew();
            var mag = _motionAreaGroupTest.CreateNew();

            obj.AsDynamic().TRUCK2MOTIONAREAGROUPID = TestDecimal;
            // Проверить
            obj.AsDynamic().TRUCK2MOTIONAREAGROUPTRUCKCODE = t.GetKey();
            obj.AsDynamic().TRUCK2MOTIONAREAGROUPMOTIONAREAGROUPCODE = mag.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRUCK2MOTIONAREAGROUPID = '{0}')", TestDecimal);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _truckTest, _motionAreaGroupTest };
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<Truck>(TestDecimal, TestString);
        }
    }
}