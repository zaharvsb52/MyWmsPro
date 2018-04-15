using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Partner2GroupTest : BaseEntityTest<Partner2Group>
    {
        protected override void FillRequiredFields(Partner2Group entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();
            
            obj.PARTNER2GROUPPARTNERGROUPID = PartnerGroupTest.ExistsItem1Id;
            obj.PARTNER2GROUPPARTNERID = TstMandantId;
            
        }
    }
}