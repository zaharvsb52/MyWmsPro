using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MMSelectTest : BaseEntityTest<MMSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MMSelectArtGroup";
        }

        protected override void FillRequiredFields(MMSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MMCode_r = MMTest.ExistsItem1Code;
            obj.Priority = TestDecimal;
        }
    }
}