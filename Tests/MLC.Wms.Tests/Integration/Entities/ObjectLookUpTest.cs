using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ObjectLookUpTest : BaseEntityTest<ObjectLookUp>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ObjectLookupFilter";
        }

        protected override void FillRequiredFields(ObjectLookUp entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ObjectLookupSource = TestString;
            obj.ObjectLookupDisplay = TestString;
            obj.ObjectLookupPkey = TestString;
            obj.ObjectLookupSimple = TestDecimal;
            obj.ObjectLookupFilter = TestString;
        }
    }
}