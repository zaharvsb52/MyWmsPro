using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TransportTaskTypeTest : BaseEntityTest<TransportTaskType>
    {
        public const string ExistsItem1Code = "TST_TRANSPORTTASKTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TTaskTypeName";
        }
    }
}