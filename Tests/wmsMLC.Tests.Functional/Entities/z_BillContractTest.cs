using System;
using System.Collections.Generic;
using FluentAssertions;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    public class BillContractTest : BaseWMSObjectTest<BillContract>
    {
        private readonly IsoCurrencyTest _isoCurrencyTest = new IsoCurrencyTest();
        private readonly VATTypeTest _vatTypeTest = new VATTypeTest();
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _isoCurrencyTest, _vatTypeTest, _mandantTest };
        }

        protected override void FillRequiredFields(BillContract obj)
        {
            base.FillRequiredFields(obj);

            var currency = _isoCurrencyTest.CreateNew();
            var vatType = _vatTypeTest.CreateNew();
            var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().CONTRACTID = TestDecimal;
            obj.AsDynamic().CONTRACTNUMBER = TestString;
            obj.AsDynamic().CONTRACTDATEFROM = DateTime.Now;
            obj.AsDynamic().CONTRACTDATETILL = DateTime.Now;
            
            obj.AsDynamic().CONTRACTOWNER = mandant.GetKey();
            obj.AsDynamic().CONTRACTCUSTOMER = mandant.GetKey();

            obj.AsDynamic().CURRENCYCODE_R = currency.GetKey();
            obj.AsDynamic().VATTYPECODE_R = vatType.GetKey();
            obj.AsDynamic().CONTRACTHOSTREF = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CONTRACTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillContract obj)
        {
            obj.AsDynamic().CONTRACTDESC = TestString;
        }

        protected override void CheckSimpleChange(BillContract source, BillContract dest)
        {
            string sourceName = source.AsDynamic().CONTRACTDESC;
            string destName = dest.AsDynamic().CONTRACTDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}