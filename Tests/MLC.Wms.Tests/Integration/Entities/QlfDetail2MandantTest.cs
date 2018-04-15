using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class QlfDetail2MandantTest : BaseEntityTest<QlfDetail2Mandant>
    {
        protected override void FillRequiredFields(QlfDetail2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.QLFDETAIL2MANDANTQLFDETAILCODE = QlfDetailTest.ExistsItem1Code;
            obj.QLFDETAIL2MANDANTPARTNERID = TstMandantId;
        }
    }
}