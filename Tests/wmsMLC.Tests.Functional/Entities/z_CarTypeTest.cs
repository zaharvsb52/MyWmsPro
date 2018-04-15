using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CarTypeTest : BaseWMSObjectTest<CarType>
    {
        protected override void FillRequiredFields(CarType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CARTYPEMARK = TestString;
            obj.AsDynamic().CARTYPEMODEL = TestString;
            obj.AsDynamic().CARTYPEID = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CARTYPEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(CarType obj)
        {
            obj.AsDynamic().CARTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(CarType source, CarType dest)
        {
            string sourceName = source.AsDynamic().CARTYPEDESC;
            string destName = dest.AsDynamic().CARTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test]
        public void Test1()
        {
            var mgr = CreateManager();
            decimal key = 337;
            var item = mgr.Get(key);
        }
    }
}