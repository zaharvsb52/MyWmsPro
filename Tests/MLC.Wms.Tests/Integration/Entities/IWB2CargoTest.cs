using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class IWB2CargoTest : BaseEntityTest<IWB2Cargo>
    {
        protected override void FillRequiredFields(IWB2Cargo entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.IWB2CARGOIWBID = IWBTest.ExistsItem1Id;
            obj.IWB2CARGOCARGOIWBID = CargoIWBTest.ExistsItem1Code;
        }
    }
}