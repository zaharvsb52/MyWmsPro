using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattTWhereEntityTest : BaseWMSObjectTest<PattTWhereEntity>
    {
        private readonly PattTWhereSectionTest _pattTWhereSectionTest = new PattTWhereSectionTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattTWhereSectionTest };
        }

        protected override void FillRequiredFields(PattTWhereEntity obj)
        {
            base.FillRequiredFields(obj);

            var ws = _pattTWhereSectionTest.CreateNew();

            obj.AsDynamic().TEMPLATEWHEREENTITYID = TestDecimal;
            obj.AsDynamic().TEMPLATEWHERESECTIONID_R = ws.GetKey();
            obj.AsDynamic().TWHEREENTITYOBJECTENTITY = "WAREHOUSE";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TEMPLATEWHEREENTITYID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattTWhereEntity obj)
        {
            obj.AsDynamic().TEMPLATEWHEREENTITYALIASENTITY = TestString;
        }

        protected override void CheckSimpleChange(PattTWhereEntity source, PattTWhereEntity dest)
        {
            string sourceName = source.AsDynamic().TEMPLATEWHEREENTITYALIASENTITY;
            string destName = dest.AsDynamic().TEMPLATEWHEREENTITYALIASENTITY;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}