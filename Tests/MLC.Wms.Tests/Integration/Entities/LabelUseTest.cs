using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class LabelUseTest : BaseEntityTest<LabelUse>
    {
        public const decimal ExistsItem1Id = -1;

        protected override void FillRequiredFields(LabelUse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.LABELCODE_R = LabelTest.ExistsItem1Code;
        }
    }
}