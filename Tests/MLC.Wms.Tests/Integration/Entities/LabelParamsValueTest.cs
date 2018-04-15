using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class LabelParamsValueTest : BaseEntityTest<LabelParamsValue>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = LabelParamsValue.LabelParamsTextPropertyName;
        }

        protected override void FillRequiredFields(LabelParamsValue entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.LABELUSEID_R = LabelUseTest.ExistsItem1Id;
            obj.LABELPARAMSID_R = LabelParamsTest.ExistsItem1Id;
        }
    }
}