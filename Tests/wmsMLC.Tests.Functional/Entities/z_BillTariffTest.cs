using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillTariffTest : BaseWMSObjectTest<BillTariff>
    {
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billOperation2ContractTest };
        }

        protected override void FillRequiredFields(BillTariff obj)
        {
            base.FillRequiredFields(obj);

            var o2c = _billOperation2ContractTest.CreateNew();

            obj.AsDynamic().TARIFFID = TestDecimal;
            obj.AsDynamic().OPERATION2CONTRACTID_R = o2c.GetKey();
            obj.AsDynamic().TARIFFDATEFROM = DateTime.Now;
            obj.AsDynamic().TARIFFDATETILL = DateTime.Now;
            obj.AsDynamic().TARIFFVALUE = TestDouble;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TARIFFID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillTariff obj)
        {
            obj.AsDynamic().TARIFFDESC = TestString;
        }

        protected override void CheckSimpleChange(BillTariff source, BillTariff dest)
        {
            string sourceName = source.AsDynamic().TARIFFDESC;
            string destName = dest.AsDynamic().TARIFFDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}