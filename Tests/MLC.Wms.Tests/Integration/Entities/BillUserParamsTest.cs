using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillUserParamsTest : BaseEntityTest<BillUserParams>
    {
        public const string ExistsItem1Code = "TST_BILLUSERPARAMS_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "USERPARAMSNAME";
        }

        protected override void FillRequiredFields(BillUserParams entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.USERPARAMSNAME = TestString;
            obj.USERPARAMSTYPECODE_R = BillUserParamsTypeTest.ExistsItem1Code;
        }
    }
}