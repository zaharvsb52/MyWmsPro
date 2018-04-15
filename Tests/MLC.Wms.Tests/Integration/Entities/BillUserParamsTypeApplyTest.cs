using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillUserParamsTypeApplyTest : BaseEntityTest<BillUserParamsTypeApply>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "USERPARAMSTYPEAPPLYNAME";
        }

        protected override void FillRequiredFields(BillUserParamsTypeApply entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.USERPARAMSTYPECODE_R = BillUserParamsTypeTest.ExistsItem1Code;
            obj.USERPARAMSTYPEAPPLYCODE = TestString.Substring(0, 30);
            obj.USERPARAMSTYPEAPPLYNAME = TestString;
        }
    }
}