using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillWorkActTest : BaseEntityTest<BillWorkAct>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BillWorkAct.WORKACTDATEFROMPropertyName;
        }

        protected override void FillRequiredFields(BillWorkAct entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CONTRACTID_R = BillContractTest.ExistsItem1Id;
            obj.WORKACTDATEFROM = DateTime.Now;
            obj.WORKACTDATETILL = DateTime.Now;
            obj.WORKACTDATE = DateTime.Now;
        }
    }
}