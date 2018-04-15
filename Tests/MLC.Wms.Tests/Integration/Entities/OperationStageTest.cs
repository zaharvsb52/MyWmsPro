using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class OperationStageTest : BaseEntityTest<OperationStage>
    {
        protected override void FillRequiredFields(OperationStage entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OperationStageName = TestString;
            obj.OperationStageFrom = TestBool;
            obj.OperationStageTo = TestBool;
            obj.OperationStageLoop = TestBool;
        }
    }
}