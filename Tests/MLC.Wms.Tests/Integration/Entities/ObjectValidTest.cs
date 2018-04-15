using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ObjectValidTest : BaseEntityTest<ObjectValid>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ObjectValidValue";
        }

        protected override void FillRequiredFields(ObjectValid entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ObjectValidName = TestString;
            obj.ObjectValidEntity = SysObjectTest.ExistsItemEntityCode;
            obj.ObjectName_r = SysObjectTest.ExistsItemEntityCode;
            obj.ObjectValidLevel = TestString;
            obj.ObjectValidPriority = TestDecimal;
        }
    }
}