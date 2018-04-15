using System;
using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SysServiceTest : BaseWMSObjectTest<SysService>
    {
        private readonly SysEnumTest _sysEnumTest = new SysEnumTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _sysEnumTest };
        }

        protected override void FillRequiredFields(SysService obj)
        {
            base.FillRequiredFields(obj);

            var en = _sysEnumTest.CreateNew();

            obj.AsDynamic().SERVICEID = TestDecimal;
            obj.AsDynamic().SERVICECODE = TestString;
            obj.AsDynamic().SERVICEGROUP = en.GetKey();
            obj.AsDynamic().SERVICETYPE = en.GetKey();
            obj.AsDynamic().SERVICEDEFAULT = false;
            obj.AsDynamic().SERVICEPRIORITY = TestDecimal;
            obj.AsDynamic().SERVICEAVAILABLEFROM = DateTime.Now;
            obj.AsDynamic().SERVICEAVAILABLETILL = DateTime.Now;
            obj.AsDynamic().SERVICELOCKED = false;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SERVICEID = '{0}')", TestDecimal);
        }
    }
}