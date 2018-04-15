using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillUserParams2O2CTest : BaseWMSObjectTest<BillUserParams2O2C>
    {
        private readonly BillUserParamsTest _billUserParamsTest = new BillUserParamsTest();
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billUserParamsTest, _billOperation2ContractTest };
        }

        protected override void FillRequiredFields(BillUserParams2O2C obj)
        {
            base.FillRequiredFields(obj);

            var up = _billUserParamsTest.CreateNew();
            var oc = _billOperation2ContractTest.CreateNew();

            obj.AsDynamic().USERPARAMS2O2CID = TestDecimal;
            obj.AsDynamic().BILLUSERPARAMS2O2CUSERPARAMSCODE = up.GetKey();
            obj.AsDynamic().BILLUSERPARAMS2O2COPERATION2CONTRACTID = oc.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERPARAMS2O2CID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillUserParams2O2C obj)
        {
            obj.AsDynamic().USERPARAMS2O2CAPPLYCODE = TestString;
        }

        protected override void CheckSimpleChange(BillUserParams2O2C source, BillUserParams2O2C dest)
        {
            string sourceName = source.AsDynamic().USERPARAMS2O2CAPPLYCODE;
            string destName = dest.AsDynamic().USERPARAMS2O2CAPPLYCODE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}