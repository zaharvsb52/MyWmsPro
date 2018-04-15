using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class OutputTest : BaseEntityTest<Output>
    {

        public const int ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OUTPUTFEEDBACK";
            HaveHistory = false;
        }

        protected override void FillRequiredFields(Output entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.Login_r = CurrentUser;
            obj.Host_r = TestString;
            obj.OutputStatus = TestString;
            obj.EpsHandler = TestDecimal;
        }

        [Test, Ignore("На PRD1 нет Transact")]
        public override void Entity_should_be_create_read_update_delete()
        {
            //base.Entity_should_be_create_read_update_delete();
        }
    }
}