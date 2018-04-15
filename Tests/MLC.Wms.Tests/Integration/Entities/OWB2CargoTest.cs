using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class OWB2CargoTest : BaseEntityTest<OWB2Cargo>
    {
        protected override void FillRequiredFields(OWB2Cargo entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OWB2CARGOOWBID = OwbTest.ExistsItem1Code;
            obj.OWB2CARGOCARGOOWBID = CargoOWBTest.ExistsItem1Code;
        }
    }
}