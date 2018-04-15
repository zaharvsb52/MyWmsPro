using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Area2BlockingTest : BaseEntityTest<Area2Blocking>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "AREA2BLOCKINGDESC";
        }

        protected override void FillRequiredFields(Area2Blocking entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.AREA2BLOCKINGAREACODE = AreaTest.ExistsItem1Code;
            // ProductBlockingTest.ExistsItem1Code нельзя использвоать, т.к. есть unique constraint на связку Area и Blocking
            obj.AREA2BLOCKINGBLOCKINGCODE = ProductBlockingTest.ExistsItem2Code;
        }
    }
}