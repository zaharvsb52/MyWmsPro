using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MotionArea2GroupTest : BaseWMSObjectTest<MotionArea2Group>
    {
        private readonly MotionAreaTest _motionAreaTest = new MotionAreaTest();
        private readonly MotionAreaGroupTest _motionAreaGroupTest = new MotionAreaGroupTest();

        protected override void FillRequiredFields(MotionArea2Group obj)
        {
            base.FillRequiredFields(obj);

            var ma = _motionAreaTest.CreateNew();
            var mag = _motionAreaGroupTest.CreateNew();

            obj.AsDynamic().MOTIONAREA2GROUPID = TestDecimal;
            obj.AsDynamic().MOTIONAREA2GROUPMOTIONAREAGROUPCODE = mag.GetKey();
            obj.AsDynamic().MOTIONAREA2GROUPMOTIONAREACODE = ma.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MOTIONAREA2GROUPID = '{0}')", TestDecimal);
        }
       
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _motionAreaTest, _motionAreaGroupTest };
        }
    }
}