using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TETest : BaseWMSObjectTest<TE>
    {
        private readonly TETypeTest _teTypeTest = new TETypeTest();
        private readonly PlaceTest _placeTest = new PlaceTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _teTypeTest, _placeTest };
        }

        protected override void FillRequiredFields(TE obj)
        {
            base.FillRequiredFields(obj);

            var t = _teTypeTest.CreateNew();
            var place = _placeTest.CreateNew();
           
            obj.AsDynamic().TECODE = TestString;
            obj.AsDynamic().TETYPECODE_R = t.GetKey();
            obj.AsDynamic().TECURRENTPLACE = place.GetKey();
            obj.AsDynamic().TECREATIONPLACE = place.GetKey();
            obj.AsDynamic().TEMAXWEIGHT = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(TE obj)
        {
            obj.AsDynamic().TEHOSTREF = TestString;
        }

        protected override void CheckSimpleChange(TE source, TE dest)
        {
            string sourceName = source.AsDynamic().TEHOSTREF;
            string destName = dest.AsDynamic().TEHOSTREF;
            sourceName.ShouldBeEquivalentTo(destName);
        }
   }
}