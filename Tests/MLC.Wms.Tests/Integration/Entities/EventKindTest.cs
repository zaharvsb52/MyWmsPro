using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EventKindTest : BaseEntityTest<EventKind>
    {
        public const string ExistsItem1Code = "TST_EVENTKIND_1";
        public const string ExistsItem2Code = "COMMACT_CLOSE";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "EVENTKINDDESC";
        }

        protected override void FillRequiredFields(EventKind entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.EVENTKINDNAME = TestString;
        }
    }
}