using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ProductBlockingTest : BaseWMSObjectTest<ProductBlocking>
    {
        protected override void FillRequiredFields(ProductBlocking obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().BLOCKINGCODE = TestString;
            obj.AsDynamic().BLOCKINGNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BLOCKINGCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(ProductBlocking obj)
        {
            obj.AsDynamic().BLOCKINGDESC = TestString;
        }

        protected override void CheckSimpleChange(ProductBlocking source, ProductBlocking dest)
        {
            string sourceName = source.AsDynamic().BLOCKINGDESC;
            string destName = dest.AsDynamic().BLOCKINGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test,Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}