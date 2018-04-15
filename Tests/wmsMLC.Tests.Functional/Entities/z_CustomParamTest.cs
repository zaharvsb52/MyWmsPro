using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CustomParamTest : BaseWMSObjectTest<CustomParam>
    {

        protected override void FillRequiredFields(CustomParam obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().CUSTOMPARAMCODE = TestString;
            obj.AsDynamic().CUSTOMPARAM2ENTITY = "MANDANT";
            obj.AsDynamic().CUSTOMPARAMNAME = TestString;
            obj.AsDynamic().CUSTOMPARAMDATATYPE = 16;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CUSTOMPARAMCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(CustomParam obj)
        {
            obj.AsDynamic().CUSTOMPARAMNAME = TestString;
        }

        protected override void CheckSimpleChange(CustomParam source, CustomParam dest)
        {
            string sourceName = source.AsDynamic().CUSTOMPARAMNAME;
            string destName = dest.AsDynamic().CUSTOMPARAMNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}