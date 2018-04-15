using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattTFieldEntityTest : BaseWMSObjectTest<PattTFieldEntity>
    {
        private readonly PattTFieldSectionTest _pattTFieldSectionTest = new PattTFieldSectionTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattTFieldSectionTest };
        }

        protected override void FillRequiredFields(PattTFieldEntity obj)
        {
            base.FillRequiredFields(obj);

            var fs = _pattTFieldSectionTest.CreateNew();

            obj.AsDynamic().TEMPLATEFIELDENTITYID = TestDecimal;
            obj.AsDynamic().TEMPLATEFIELDSECTIONID_R = fs.GetKey();
            obj.AsDynamic().TFIELDENTITYOBJECTENTITY = "WAREHOUSE";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TEMPLATEFIELDENTITYID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattTFieldEntity obj)
        {
            obj.AsDynamic().TEMPLATEFIELDENTITYALIASENTITY = TestString;
        }

        protected override void CheckSimpleChange(PattTFieldEntity source, PattTFieldEntity dest)
        {
            string sourceName = source.AsDynamic().TEMPLATEFIELDENTITYALIASENTITY;
            string destName = dest.AsDynamic().TEMPLATEFIELDENTITYALIASENTITY;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}