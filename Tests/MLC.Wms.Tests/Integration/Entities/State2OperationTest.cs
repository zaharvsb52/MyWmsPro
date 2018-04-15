using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class State2OperationTest : BaseEntityTest<State2Operation>
    {
        protected override void FillRequiredFields(State2Operation entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.STATE2OPERATIONOBJECTNAME = SysObjectTest.ExistsItemEntityCode;
            obj.STATE2OPERATIONOPERATIONCODE = BillOperationTest.ExistsItem1Code;
            obj.STATE2OPERATIONSTATUSCODE = StatusTest.ExistsItem1Code;
            obj.STATE2OPERATIONDISABLE = TestBool;
        }
    }
}