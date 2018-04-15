using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CustomParamTest : BaseEntityTest<CustomParam>
    {
        public const string ExistsItem1Code = "TST_CUSTOMPARAM_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = CustomParam.CustomParamNamePropertyName;
        }

        protected override void FillRequiredFields(CustomParam entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CUSTOMPARAMCODE = TestString;
            obj.CUSTOMPARAM2ENTITY = SysObjectTest.ExistsItemEntityCode;
            obj.CUSTOMPARAMNAME = TestString;
            obj.CUSTOMPARAMDATATYPE = SysObjectTest.ExistsItem1Code;
        }
    }
}