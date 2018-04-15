using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WorkTest : BaseEntityTest<Work>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WorkDesc";
        }

        protected override void FillRequiredFields(Work entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WorkGroupID_r = WorkGroupTest.ExistsItem1Id;
            obj.OperationCode_r = BillOperationTest.ExistsItem1Code;
            obj.StatusCode_r = StatusTest.ExistsItem1Code;
            obj.ClientSessionID_r = ClientSessionTest.ExistsItem1Id;
        }
    }
}