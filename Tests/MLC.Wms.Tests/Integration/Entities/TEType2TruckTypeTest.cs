using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TEType2TruckTypeTest : BaseEntityTest<TEType2TruckType>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TETYPE2TRUCKTYPEDESC";
        }

        protected override void FillRequiredFields(TEType2TruckType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TETYPE2TRUCKTYPETETYPECODE = TETypeTest.ExistsItem1Code;
            obj.TETYPE2TRUCKTYPETRUCKTYPECODE = TruckTypeTest.ExistsItem1Code;
            obj.TETYPE2TRUCKTYPECOUNT = TestDecimal;
        }
    }
}