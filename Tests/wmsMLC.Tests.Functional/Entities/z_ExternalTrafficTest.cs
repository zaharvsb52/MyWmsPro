using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ExternalTrafficTest : BaseWMSObjectTest<ExternalTraffic>
    {
        private readonly VehicleTest _vehicleTest = new VehicleTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _vehicleTest };
        }

        protected override void FillRequiredFields(ExternalTraffic obj)
        {
            base.FillRequiredFields(obj);

            var vehicle = _vehicleTest.CreateNew();
            
            obj.AsDynamic().EXTERNALTRAFFICID = TestDecimal;
            obj.AsDynamic().VEHICLEID_R = vehicle.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EXTERNALTRAFFICID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(ExternalTraffic obj)
        {
            obj.AsDynamic().EXTERNALTRAFFICTRAILERRN = TestString;
        }

        protected override void CheckSimpleChange(ExternalTraffic source, ExternalTraffic dest)
        {
            string sourceName = source.AsDynamic().EXTERNALTRAFFICTRAILERRN;
            string destName = dest.AsDynamic().EXTERNALTRAFFICTRAILERRN;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}