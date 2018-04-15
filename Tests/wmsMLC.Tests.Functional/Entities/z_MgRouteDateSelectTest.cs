using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MgRouteDateSelectTest : BaseWMSObjectTest<MgRouteDateSelect>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(MgRouteDateSelect obj)
        {
            base.FillRequiredFields(obj);

            var m = _mandantTest.CreateNew();

            obj.AsDynamic().MGROUTEDATESELECTID = TestDecimal;
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().MANDANTID = m.GetKey();
            obj.AsDynamic().MGROUTEDATESELECTFROM = DateTime.Now;
            obj.AsDynamic().MGROUTEDATESELECTTILL = DateTime.Now;
            obj.AsDynamic().MGROUTEDATESELECTDATESOURCE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MGROUTEDATESELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MgRouteDateSelect obj)
        {
            obj.AsDynamic().MGROUTEDATESELECTREGION = TestString;
        }

        protected override void CheckSimpleChange(MgRouteDateSelect source, MgRouteDateSelect dest)
        {
            string sourceName = source.AsDynamic().MGROUTEDATESELECTREGION;
            string destName = dest.AsDynamic().MGROUTEDATESELECTREGION;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}