using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class QlfTest : BaseWMSObjectTest<Qlf>
    {
        protected override void FillRequiredFields(Qlf obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().QLFCODE = TestString;
            obj.AsDynamic().QLFNAME = TestString;
            obj.AsDynamic().QLFTYPE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(QLFCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Qlf obj)
        {
            obj.AsDynamic().QLFDESC = TestString;
        }

        protected override void CheckSimpleChange(Qlf source, Qlf dest)
        {
            string sourceName = source.AsDynamic().QLFDESC;
            string destName = dest.AsDynamic().QLFDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}