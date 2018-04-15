using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class IsoCountryTest : BaseWMSObjectTest<IsoCountry>
    {
        public IsoCountryTest()
        {
            TestString = "TST";
        }

        protected override void FillRequiredFields(IsoCountry obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().COUNTRYCODE = TestString;
            obj.AsDynamic().COUNTRYNAMERUS = TestString;
            obj.AsDynamic().COUNTRYALPHA2 = TestString.Substring(0, 2);
            obj.AsDynamic().COUNTRYNUMERIC = TestString;
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(COUNTRYCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(IsoCountry obj)
        {
            obj.AsDynamic().COUNTRYNAMEENG = TestString;
        }

        protected override void CheckSimpleChange(IsoCountry source, IsoCountry dest)
        {
            string sourceName = source.AsDynamic().COUNTRYNAMEENG;
            string destName = dest.AsDynamic().COUNTRYNAMEENG;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}