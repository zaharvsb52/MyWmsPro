using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CustomParamValueTest : BaseEntityTest<CustomParamValue>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = CustomParamValue.CPVValuePropertyName;
        }

        protected override void FillRequiredFields(CustomParamValue entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();
            
            obj.CPV2Entity = SysObjectTest.ExistsItemEntityCode;
            obj.CPVKey = TestString;
            obj.CPVValue = TestString;
            obj.CustomParamCode = CustomParamTest.ExistsItem1Code;
        }
    }
}