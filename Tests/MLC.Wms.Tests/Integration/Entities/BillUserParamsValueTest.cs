using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillUserParamsValueTest : BaseEntityTest<BillUserParamsValue>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "USERPARAMSVALUEDATEFROM";
        }

        protected override void FillRequiredFields(BillUserParamsValue entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.USERPARAMSCODE_R = BillUserParamsTest.ExistsItem1Code;
            obj.USERPARAMSVALUEDATEFROM = DateTime.Now;
            obj.USERPARAMSVALUEDATETILL = DateTime.Now;
        }
    }
}