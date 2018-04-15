using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TruckTest : BaseWMSObjectTest<Truck>
    {
        private readonly TruckTypeTest _truckTypeTest = new TruckTypeTest();

        protected override void FillRequiredFields(Truck obj)
        {
            base.FillRequiredFields(obj);

            var tt = _truckTypeTest.CreateNew();

            obj.AsDynamic().TRUCKCODE = TestString;
            obj.AsDynamic().TRUCKTYPECODE_R = tt.GetKey();
            obj.AsDynamic().TRUCKNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRUCKCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Truck obj)
        {
            obj.AsDynamic().TRUCKDESC = TestString;
        }

        protected override void CheckSimpleChange(Truck source, Truck dest)
        {
            string sourceName = source.AsDynamic().TRUCKDESC;
            string destName = dest.AsDynamic().TRUCKDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _truckTypeTest };
        }
    }
}