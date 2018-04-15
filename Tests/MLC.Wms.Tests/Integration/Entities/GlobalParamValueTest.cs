using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class GlobalParamValueTest : BaseEntityTest<GlobalParamValue>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = GlobalParamValue.GparamValValuePropertyName;
        }

        protected override void FillRequiredFields(GlobalParamValue entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.GLOBALPARAMCODE_R = GlobalParamTest.ExistsItem1Code;
            obj.GPARAMVAL2ENTITY = "GLOBALPARAMVALUE";
            obj.GPARAMVALKEY = TestDecimal;
        }
    }
}