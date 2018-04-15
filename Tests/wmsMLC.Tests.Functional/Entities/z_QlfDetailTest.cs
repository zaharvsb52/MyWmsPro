using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class QlfDetailTest : BaseWMSObjectTest<QlfDetail>
    {
        protected override void FillRequiredFields(QlfDetail obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().QLFDETAILCODE = TestString;
            obj.AsDynamic().QLFDETAILNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(QLFDETAILCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(QlfDetail obj)
        {
            obj.AsDynamic().QLFDETAILDESC = TestString;
        }

        protected override void CheckSimpleChange(QlfDetail source, QlfDetail dest)
        {
            string sourceName = source.AsDynamic().QLFDETAILDESC;
            string destName = dest.AsDynamic().QLFDETAILDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}