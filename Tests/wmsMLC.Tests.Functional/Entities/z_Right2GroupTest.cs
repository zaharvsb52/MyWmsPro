using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Right2GroupTest : BaseWMSObjectTest<Right2Group>
    {
        private readonly RightGroupTest _rightGroupTest = new RightGroupTest();
        private readonly  RightTest _rightTest = new RightTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _rightGroupTest, _rightTest };
        }
        
        protected override string GetCheckFilter()
        {
            return string.Format("(RIGHT2GROUPID = '{0}')", TestDecimal);
        }

        protected override void FillRequiredFields(Right2Group obj)
        {
            base.FillRequiredFields(obj);
            _rightTest.TestString = TestString;
            var r = _rightTest.CreateNew();
            _rightGroupTest.TestString = TestString;
            var rg = _rightGroupTest.CreateNew();
            
            obj.AsDynamic().RIGHT2GROUPID = TestDecimal;
            obj.AsDynamic().RIGHT2GROUPRIGHTCODE = r.GetKey();
            obj.AsDynamic().RIGHT2GROUPRIGHTGROUPCODE = rg.GetKey();
        }

        protected override void MakeSimpleChange(Right2Group obj)
        {
        }

        protected override void CheckSimpleChange(Right2Group source, Right2Group dest)
        {
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<RightGroup>(TestDecimal, TestString);
        }

        [Test,Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {

        }
    }
}

