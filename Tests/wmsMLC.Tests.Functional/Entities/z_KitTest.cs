using FluentAssertions;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    public class KitTest : BaseWMSObjectTest<Kit>
    {
        private readonly KitTypeTest _kitTypeTest = new KitTypeTest();

        public KitTest()
        {
            _kitTypeTest.TestString = TestString;
        }

        protected override void FillRequiredFields(Kit obj)
        {
            base.FillRequiredFields(obj);

            var kitType = _kitTypeTest.CreateNew();

            obj.AsDynamic().KITCODE = TestString;
            obj.AsDynamic().KITTYPECODE_R = kitType.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(KITCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Kit obj)
        {
            obj.AsDynamic().KITPRIORITYOUT = TestDecimal;
        }

        protected override void CheckSimpleChange(Kit source, Kit dest)
        {
            decimal sourceName = source.AsDynamic().KITPRIORITYOUT;
            decimal destName = dest.AsDynamic().KITPRIORITYOUT;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();

            _kitTypeTest.ClearForSelf();
        }
    }
}