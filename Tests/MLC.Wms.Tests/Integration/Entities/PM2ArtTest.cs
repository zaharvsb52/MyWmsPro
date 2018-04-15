using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PM2ArtTest : BaseEntityTest<PM2Art>
    {
        protected override void FillRequiredFields(PM2Art entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PM2ARTPMCODE = PMTest.ExistsItem1Code;
        }
    }
}