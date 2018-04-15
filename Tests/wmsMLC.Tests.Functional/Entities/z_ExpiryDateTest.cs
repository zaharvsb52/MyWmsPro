using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ExpiryDateTest : BaseWMSObjectTest<ExpiryDate>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(ExpiryDate obj)
        {
            base.FillRequiredFields(obj);

            var m = _mandantTest.CreateNew();

            obj.AsDynamic().EXPIRYDATEID = TestDecimal;
            obj.AsDynamic().MANDANTID = m.GetKey();
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().EXPIRYDATETYPE = TestString;
            obj.AsDynamic().EXPIRYDATEVALUE = TestDecimal;
            obj.AsDynamic().EXPIRYDATEVALUETYPE = TestString;
            obj.AsDynamic().EXPIRYDATEUSINGOPTION = TestString;
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EXPIRYDATEID = '{0}')", TestDecimal);
        }
    }
}