using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WorkGroupTest : BaseEntityTest<WorkGroup>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WorkGroupType";
        }

        protected override void FillRequiredFields(WorkGroup entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MandantID = TstMandantId;
            obj.WorkGroupCode = TestString;
        }
    }
}