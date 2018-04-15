using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PM2OperationTest : BaseEntityTest<PM2Operation>
    {
        public const string ExistsItem1Code = "TST_PM2OPERATION_1";

        protected override void FillRequiredFields(PM2Operation entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PM2OPERATIONPMCODE = PMTest.ExistsItem2Code;
            obj.PM2OPERATIONOPERATIONCODE = BillOperationTest.ExistsItem1Code;
        }
    }
}