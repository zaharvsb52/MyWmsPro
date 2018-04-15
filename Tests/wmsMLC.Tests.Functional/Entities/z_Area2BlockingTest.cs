using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Area2BlockingTest : BaseWMSObjectTest<Area2Blocking>
    {
        private readonly AreaTest _areaTest = new AreaTest();
        private readonly ProductBlockingTest _productBlockingTest = new ProductBlockingTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _areaTest, _productBlockingTest };
        }

        protected override void FillRequiredFields(Area2Blocking obj)
        {
            base.FillRequiredFields(obj);

            var area = _areaTest.CreateNew();
            var block = _productBlockingTest.CreateNew(bl =>
            {
                bl.AsDynamic().BLOCKINGFORPLACE = true;
            });

            obj.AsDynamic().AREA2BLOCKINGID = TestDecimal;
            obj.AsDynamic().AREA2BLOCKINGAREACODE = area.GetKey();
            obj.AsDynamic().AREA2BLOCKINGBLOCKINGCODE = block.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(AREA2BLOCKINGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Area2Blocking obj)
        {
            obj.AsDynamic().AREA2BLOCKINGDESC = TestString;
        }

        protected override void CheckSimpleChange(Area2Blocking source, Area2Blocking dest)
        {
            string sourceName = source.AsDynamic().AREA2BLOCKINGDESC;
            string destName = dest.AsDynamic().AREA2BLOCKINGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет истории")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}
