using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class GCTest : BaseWMSObjectTest<GlobalConfig>
    {
        protected override void FillRequiredFields(GlobalConfig obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().GCCODE = TestGuid;
            obj.AsDynamic().GCENTITY = TestString;
            obj.AsDynamic().GCKEY = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(GCCODE = '{0}')", TestGuid.ToString("N"));
        }

        protected override void MakeSimpleChange(GlobalConfig obj)
        {
            obj.AsDynamic().GCENTITY = TestString;
        }

        protected override void CheckSimpleChange(GlobalConfig source, GlobalConfig dest)
        {
            string sourceName = source.AsDynamic().GCENTITY;
            string destName = dest.AsDynamic().GCENTITY;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}