using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ReceiveAreaTest : BaseEntityTest<ReceiveArea>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ReceiveAreaDesc";
        }

        protected override void FillRequiredFields(ReceiveArea entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ReceiveAreaName = TestString;
        }
    }
}