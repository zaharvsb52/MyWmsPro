using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SysArchInstTest : BaseWMSObjectTest<SysArchInst>
    {
        private readonly z_SysArchTest _sysArchTest = new z_SysArchTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _sysArchTest };
        }

        protected override void FillRequiredFields(SysArchInst obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().ARCHINSTGUID = TestGuid;
            //obj.AsDynamic().ARCHCODE_R = _sysArchTest.CreateNew().GetKey();
            obj.AsDynamic().ARCHCODE_R = "SYSEVENT_O2H";
            obj.AsDynamic().ARCHINSTDATE = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ARCHINSTGUID = '{0}')", TestGuid.ToString("N"));
        }

        protected override void MakeSimpleChange(SysArchInst obj)
        {
            obj.AsDynamic().ARCHINSTCOUNT = TestDecimal;
        }

        protected override void CheckSimpleChange(SysArchInst source, SysArchInst dest)
        {
            decimal sourceName = source.AsDynamic().ARCHINSTCOUNT;
            decimal destName = dest.AsDynamic().ARCHINSTCOUNT;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}