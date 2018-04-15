using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MgRouteTest : BaseWMSObjectTest<MgRoute>
    {
        private readonly MandantTest _mandantTestTest = new MandantTest();
        private readonly GateTest _gateTest = new GateTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTestTest, _gateTest };
        }

        protected override void FillRequiredFields(MgRoute obj)
        {
            base.FillRequiredFields(obj);

            var gate = _gateTest.CreateNew();
            var mandant = _mandantTestTest.CreateNew();
            
            obj.AsDynamic().MGROUTEID = TestDecimal;
            obj.AsDynamic().MANDANTID = mandant.GetKey();
            obj.AsDynamic().MGROUTENAME = TestString;
            obj.AsDynamic().MGROUTENUMBER = TestString;
            obj.AsDynamic().GATECODE_R = gate.GetKey();
            obj.AsDynamic().MGROUTECREATEROUTE = false;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MGROUTEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MgRoute obj)
        {
            obj.AsDynamic().MGROUTEDESC = TestString;
        }

        protected override void CheckSimpleChange(MgRoute source, MgRoute dest)
        {
            string sourceName = source.AsDynamic().MGROUTEDESC;
            string destName = dest.AsDynamic().MGROUTEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}