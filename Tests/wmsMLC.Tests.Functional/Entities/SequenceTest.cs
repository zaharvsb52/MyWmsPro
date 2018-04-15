using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SequenceTest : BaseWMSObjectTest<Sequence>
    {
        // Пока не запускаем изменяющие тесты
        public override void ManagerCRUDTest() {}
        public override void ManagerGetAllTest() {}
        public override void ManagerGetFilteredTest() {}

        protected override void FillRequiredFields(Sequence obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().SEQUENCECODE = TestString;
            obj.AsDynamic().SEQUENCEMINVALUE = TestDecimal;
            obj.AsDynamic().SEQUENCEMAXVALUE = TestDecimal * 10;
            obj.AsDynamic().SEQUENCESTARTVALUE = TestDecimal;
            obj.AsDynamic().SEQUENCECACHE = TestDecimal;
            obj.AsDynamic().SEQUENCEINCREMENT = TestDecimal;
            obj.AsDynamic().SEQUENCENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SEQUENCECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Sequence obj)
        {
            obj.AsDynamic().SEQUENCEDESC = TestString;
        }

        protected override void CheckSimpleChange(Sequence source, Sequence dest)
        {
            string sourceName = source.AsDynamic().SEQUENCEDESC;
            string destName = dest.AsDynamic().SEQUENCEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}