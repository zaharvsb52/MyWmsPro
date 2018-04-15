using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WorkGroupTest : BaseWMSObjectTest<WorkGroup>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(WorkGroup obj)
        {
            base.FillRequiredFields(obj);

            var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().WORKGROUPID = TestDecimal;
            obj.AsDynamic().WORKGROUPCODE = TestString;
            obj.AsDynamic().MANDANTID = mandant.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKGROUPID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(WorkGroup obj)
        {
            obj.AsDynamic().WORKGROUPTYPE = TestString;
        }

        protected override void CheckSimpleChange(WorkGroup source, WorkGroup dest)
        {
            string sourceName = source.AsDynamic().WORKGROUPTYPE;
            string destName = dest.AsDynamic().WORKGROUPTYPE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}