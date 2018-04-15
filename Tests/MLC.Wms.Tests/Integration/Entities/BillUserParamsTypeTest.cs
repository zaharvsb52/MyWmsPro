using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillUserParamsTypeTest : BaseEntityTest<BillUserParamsType>
    {
        public const string ExistsItem1Code = "TST_BILLUSERPARAMSTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "USERPARAMSTYPENAME";
        }

        protected override void FillRequiredFields(BillUserParamsType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.USERPARAMSTYPENAME = TestString;
            obj.USERPARAMSTYPERANGETYPE = TestString;
            obj.USERPARAMSTYPERANGEDATATYPE = TestDecimal;
            obj.USERPARAMSTYPEVALUEDATATYPE = TestDecimal;
            obj.USERPARAMSTYPEUSINGOPTION = TestString;
        }
    }
}