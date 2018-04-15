using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattTFieldTest : BaseWMSObjectTest<PattTField>
    {
        private readonly PattTFieldSectionTest _pattTFieldSectionTest = new PattTFieldSectionTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattTFieldSectionTest };
        }

        protected override void FillRequiredFields(PattTField obj)
        {
            base.FillRequiredFields(obj);

            var fs = _pattTFieldSectionTest.CreateNew();

            obj.AsDynamic().TEMPLATEFIELDID = TestDecimal;
            obj.AsDynamic().TEMPLATEFIELDSECTIONID_R = fs.GetKey();
            obj.AsDynamic().TEMPLATEFIELDNAME = TestString;
            obj.AsDynamic().TEMPLATEFIELDALIAS = TestString;
            obj.AsDynamic().TEMPLATEFIELDDATATYPE = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TEMPLATEFIELDID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattTField obj)
        {
            obj.AsDynamic().TEMPLATEFIELDDESC = TestString;
        }

        protected override void CheckSimpleChange(PattTField source, PattTField dest)
        {
            string sourceName = source.AsDynamic().TEMPLATEFIELDDESC;
            string destName = dest.AsDynamic().TEMPLATEFIELDDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}