using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Qlf2MandantTest : BaseEntityTest<Qlf2Mandant>
    {
        protected override void FillRequiredFields(Qlf2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.QLF2MANDANTQLFCODE = QlfTest.ExistsItem1Code;
            obj.QLF2MANDANTPARTNERID = TstMandantId;
        }
    }
}