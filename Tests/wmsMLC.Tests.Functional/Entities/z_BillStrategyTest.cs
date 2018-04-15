using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillStrategyTest : BaseWMSObjectTest<BillStrategy>
    {
        protected override void FillRequiredFields(BillStrategy obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().STRATEGYCODE = TestString;
            obj.AsDynamic().STRATEGYNAME = TestString;
            obj.AsDynamic().STRATEGYGROUP = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(STRATEGYCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillStrategy obj)
        {
            obj.AsDynamic().STRATEGYDESC = TestString;
        }

        protected override void CheckSimpleChange(BillStrategy source, BillStrategy dest)
        {
            string sourceName = source.AsDynamic().STRATEGYDESC;
            string destName = dest.AsDynamic().STRATEGYDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}