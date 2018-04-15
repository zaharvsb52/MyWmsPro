using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EventKind2MandantTest : BaseEntityTest<EventKind2Mandant>
    {
        protected override void FillRequiredFields(EventKind2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.EVENTKIND2MANDANTEVENTKINDCODE = EventKindTest.ExistsItem1Code;
            obj.EVENTKIND2MANDANTPARTNERID = TstMandantId;
        }
    }
}