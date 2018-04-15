using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MgRouteDateSelectTest : BaseEntityTest<MgRouteDateSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MgRouteDateSelectRegion";
        }

        protected override void FillRequiredFields(MgRouteDateSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();
           
            obj.Priority = TestDecimal;
            obj.MgRouteDateSelectDateSource = TestString;
        }
    }
}