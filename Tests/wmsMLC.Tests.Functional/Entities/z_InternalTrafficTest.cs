using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public sealed class InternalTrafficTest : BaseWMSObjectTest<InternalTraffic>
    {
        private readonly ExternalTrafficTest _trafficTest = new ExternalTrafficTest();
        private readonly WarehouseTest _warehouseTest = new WarehouseTest();
        private readonly PurposeVisitTest _purposeVisitTest = new PurposeVisitTest();
        private readonly StatusTest _statusTest = new StatusTest();
        // private readonly MandantTest _mandantTest = new MandantTest();

        public InternalTrafficTest()
        {
            _trafficTest.TestString = TestString;
            _warehouseTest.TestString = TestString;
        }

        protected override void FillRequiredFields(InternalTraffic obj)
        {
            base.FillRequiredFields(obj);

            var traffic = _trafficTest.CreateNew();
            var warehouse = _warehouseTest.CreateNew();
            var visit = _purposeVisitTest.CreateNew();
            var status = _statusTest.CreateNew();
            //var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().INTERNALTRAFFICORDER = TestDecimal;
            obj.AsDynamic().EXTERNALTRAFFICID_R = traffic.GetKey();
            obj.AsDynamic().WAREHOUSECODE_R = warehouse.GetKey();
            obj.AsDynamic().PURPOSEVISITID_R = visit.GetKey();
            obj.AsDynamic().STATUSCODE_R = status.GetKey();
            obj.AsDynamic().MANDANTID = 1;// mandant.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INTERNALTRAFFICORDER = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(InternalTraffic obj)
        {
            obj.AsDynamic().INTERNALTRAFFICORDER = TestDecimal;
        }

        protected override void CheckSimpleChange(InternalTraffic source, InternalTraffic dest)
        {
            decimal sourceName = source.AsDynamic().INTERNALTRAFFICORDER;
            decimal destName = dest.AsDynamic().INTERNALTRAFFICORDER;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _trafficTest, _warehouseTest, _purposeVisitTest, _statusTest/*, _mandantTest*/ };
        }
    }
}