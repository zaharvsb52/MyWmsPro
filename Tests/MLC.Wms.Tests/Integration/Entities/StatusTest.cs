using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class StatusTest : BaseEntityTest<Status>
    {
        public const string ExistsItem1Code = "TST_STATUS_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Status.StatusNamePropertyName;
        }

        protected override void FillRequiredFields(Status obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().STATUSNAME = TestString;
            obj.AsDynamic().STATUS2ENTITY = "STATUS";
        }
    }
}