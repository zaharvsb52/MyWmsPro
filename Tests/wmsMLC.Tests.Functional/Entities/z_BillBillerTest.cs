using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillBillerTest : BaseWMSObjectTest<BillBiller>
    {
        protected override void FillRequiredFields(BillBiller obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().BILLERCODE = TestString;
            obj.AsDynamic().BILLERNAME = TestString;
            obj.AsDynamic().BILLERLOCKED = false;
            obj.AsDynamic().BILLERPROCEDURECALC = TestString;
            obj.AsDynamic().BILLERPROCEDUREAFTER = TestString;
            obj.AsDynamic().BILLERCRON = TestString;
            obj.AsDynamic().BILLERORDER = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BILLERCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillBiller obj)
        {
            obj.AsDynamic().BILLERPROCEDUREBEFORE = TestString;
        }

        protected override void CheckSimpleChange(BillBiller source, BillBiller dest)
        {
            string sourceName = source.AsDynamic().BILLERPROCEDUREBEFORE;
            string destName = dest.AsDynamic().BILLERPROCEDUREBEFORE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}