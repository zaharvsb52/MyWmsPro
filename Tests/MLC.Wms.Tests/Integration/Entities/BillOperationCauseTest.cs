using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillOperationCauseTest : BaseEntityTest<BillOperationCause>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillOperationCause.OperationCauseNamePropertyName;
        }

        protected override void FillRequiredFields(BillOperationCause entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OPERATION2CONTRACTID_R = BillOperation2ContractTest.ExistsItem1Id;
            obj.OPERATIONCAUSENAME = TestString;
        }

        [Test, Ignore("На PRD1 нет Transact")]
        public override void Entity_should_be_create_read_update_delete()
        {
            base.Entity_should_be_create_read_update_delete();
        }
    }
}