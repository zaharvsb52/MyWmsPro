using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture, Ignore]
    public class BillUserParamsTypeApplyTest : BaseWMSObjectTest<BillUserParamsTypeApply>
    {
        private readonly BillUserParamsTypeTest _billUserParamsTypeTest = new BillUserParamsTypeTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billUserParamsTypeTest };
        }

        protected override void FillRequiredFields(BillUserParamsTypeApply obj)
        {
            base.FillRequiredFields(obj);

            var upt = _billUserParamsTypeTest.CreateNew();

            obj.AsDynamic().USERPARAMSTYPEAPPLYID = TestDecimal;
            obj.AsDynamic().USERPARAMSTYPEAPPLYCODE = TestString;
            obj.AsDynamic().USERPARAMSTYPEAPPLYNAME = TestString;
            obj.AsDynamic().USERPARAMSTYPECODEID_R = upt.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERPARAMSTYPEAPPLYID = '{0}')", TestDecimal);
        }
    }
}