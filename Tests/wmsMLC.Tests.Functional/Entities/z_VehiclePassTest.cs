using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class VehiclePassTest : BaseWMSObjectTest<VehiclePass>
    {
        private readonly VehicleTest _vehicleTest = new VehicleTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _vehicleTest };
        }


        protected override void FillRequiredFields(VehiclePass obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().VEHICLEPASSID = TestDecimal;
            obj.AsDynamic().VEHICLEID_R = _vehicleTest.CreateNew().GetKey();
            obj.AsDynamic().VEHICLEPASSTYPE = TestString;
            obj.AsDynamic().VEHICLEPASSSERIES = TestString;
            obj.AsDynamic().VEHICLEPASSNUMBER = TestString;
            obj.AsDynamic().VEHICLEPASSRECEIPT = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(VEHICLEPASSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(VehiclePass obj)
        {
            obj.AsDynamic().VEHICLEPASSAGENCY = TestString;
        }

        protected override void CheckSimpleChange(VehiclePass source, VehiclePass dest)
        {
            string sourceName = source.AsDynamic().VEHICLEPASSAGENCY;
            string destName = dest.AsDynamic().VEHICLEPASSAGENCY;
            sourceName.ShouldBeEquivalentTo(destName);
        }

    }
}