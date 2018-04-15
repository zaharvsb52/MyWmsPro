using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RouteTest : BaseWMSObjectTest<Route>
    {
        private readonly MandantTest _mandantTest = new MandantTest();
        private readonly GateTest _gateTest = new GateTest();
        private readonly MgRouteTest _mgRouteTest = new MgRouteTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest, _gateTest, _mgRouteTest };
        }

        protected override void FillRequiredFields(Route obj)
        {
            base.FillRequiredFields(obj);

            var gate = _gateTest.CreateNew();
            var mandant = _mandantTest.CreateNew();
            _mgRouteTest.TestDecimal = TestDecimal + 1;
            _mgRouteTest.TestString = TestString + "1";
            var mgRoute = _mgRouteTest.CreateNew();

            obj.AsDynamic().ROUTEID = TestDecimal;
            obj.AsDynamic().MANDANTID = mandant.GetKey();
            obj.AsDynamic().ROUTENUMBER = "123";
            obj.AsDynamic().MGROUTEID_R = mgRoute.GetKey();
            obj.AsDynamic().ROUTEDATE = DateTime.Now;
            obj.AsDynamic().GATECODE_R = gate.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ROUTEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Route obj)
        {
            obj.AsDynamic().ROUTEHOSTREF = TestString;
        }

        protected override void CheckSimpleChange(Route source, Route dest)
        {
            string sourceName = source.AsDynamic().ROUTEHOSTREF;
            string destName = dest.AsDynamic().ROUTEHOSTREF;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}