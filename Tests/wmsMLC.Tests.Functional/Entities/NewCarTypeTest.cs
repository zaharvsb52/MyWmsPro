using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class NewCarTypeTest : NewBaseWMSObjectTest<CarType>
    {
        protected override void FillRequaredFields(CarType obj)
        {
            base.FillRequaredFields(obj);

            obj.AsDynamic().CARTYPEID = TestDecimal;
            obj.AsDynamic().CARTYPEMARK = TestString;
            obj.AsDynamic().CARTYPEMODEL = TestString;
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
    }
}