using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PartnerGroupTest : BaseEntityTest<PartnerGroup>
    {
        public const decimal ExistsItem1Id = -1;

        protected override void FillRequiredFields(PartnerGroup entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();

            obj.PARTNERGROUPNAME = TestString;
        }
    }
}