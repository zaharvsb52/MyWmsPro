using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillUserParamsValueTest : BaseWMSObjectTest<BillUserParamsValue>
    {
        private readonly BillUserParamsTest _billUserParamsTest = new BillUserParamsTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billUserParamsTest };
        }

        protected override void FillRequiredFields(BillUserParamsValue obj)
        {
            base.FillRequiredFields(obj);

            var param = _billUserParamsTest.CreateNew();

            obj.AsDynamic().USERPARAMSVALUEID = TestDecimal;
            obj.AsDynamic().USERPARAMSCODE_R = param.GetKey();
            obj.AsDynamic().USERPARAMSVALUEDATEFROM = DateTime.Now;
            obj.AsDynamic().USERPARAMSVALUEDATETILL = DateTime.Now;
            obj.AsDynamic().USERPARAMSVALUETILL = TestString;
            obj.AsDynamic().USERPARAMSVALUEVALUE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERPARAMSVALUEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillUserParamsValue obj)
        {
            obj.AsDynamic().USERPARAMSVALUEFROM= TestString;
        }

        protected override void CheckSimpleChange(BillUserParamsValue source, BillUserParamsValue dest)
        {
            string sourceName = source.AsDynamic().USERPARAMSVALUEFROM;
            string destName = dest.AsDynamic().USERPARAMSVALUEFROM;
            sourceName.ShouldBeEquivalentTo(destName);
        }
      
    }
}