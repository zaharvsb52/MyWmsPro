using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TE2BlockingTest : BaseWMSObjectTest<TE2Blocking>
    {
        private readonly TETest _teTest = new TETest();
        private  readonly ProductBlockingTest _productBlockingTest = new ProductBlockingTest();

        public TE2BlockingTest()
        {
            _teTest.TestString = TestString;
            _productBlockingTest.TestString = TestString;
        }

        protected override void FillRequiredFields(TE2Blocking obj)
        {
            base.FillRequiredFields(obj);

            var t = _teTest.CreateNew();
            var p = _productBlockingTest.CreateNew(pb =>
                {
                    pb.AsDynamic().BLOCKINGFORTE = true;
                });

            obj.AsDynamic().TE2BLOCKINGID = TestDecimal;
            obj.AsDynamic().TE2BLOCKINGTECODE = t.GetKey();
            obj.AsDynamic().TE2BLOCKINGBLOCKINGCODE = p.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TE2BLOCKINGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(TE2Blocking obj)
        {
            obj.AsDynamic().TE2BLOCKINGDESC = TestString;
        }

        protected override void CheckSimpleChange(TE2Blocking source, TE2Blocking dest)
        {
            string sourceName = source.AsDynamic().TE2BLOCKINGDESC;
            string destName = dest.AsDynamic().TE2BLOCKINGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _teTest.ClearForSelf();
            _productBlockingTest.ClearForSelf();
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<TE>(TestDecimal, TestString);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}