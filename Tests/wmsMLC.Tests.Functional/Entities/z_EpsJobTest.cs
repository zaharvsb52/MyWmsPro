using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class EpsJobTest : BaseWMSObjectTest<EpsJob>
    {
        public const bool TestBool = false;

        protected override string GetCheckFilter()
        {
            return string.Format("(JOBCODE = '{0}')", TestString);
        }

        protected override void FillRequiredFields(EpsJob obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().JOBCODE = TestString;
            obj.AsDynamic().JOBLOCKED = TestBool;
        }

        protected override void MakeSimpleChange(EpsJob obj)
        {
            obj.AsDynamic().JOBNAME = TestString;
        }

        protected override void CheckSimpleChange(EpsJob source, EpsJob dest)
        {
            string sourceName = source.AsDynamic().JOBNAME;
            string destName = dest.AsDynamic().JOBNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}