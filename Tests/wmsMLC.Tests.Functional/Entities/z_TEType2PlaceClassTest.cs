using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TEType2PlaceClassTest : BaseWMSObjectTest<TEType2PlaceClass>
    {
        private readonly TETypeTest _teTypeTest = new TETypeTest();
        private readonly PlaceClassTest _placeClassTest = new PlaceClassTest();


        protected override void FillRequiredFields(TEType2PlaceClass obj)
        {
            base.FillRequiredFields(obj);

            var t = _teTypeTest.CreateNew();
            var p = _placeClassTest.CreateNew();

            obj.AsDynamic().TETYPE2PLACECLASSID = TestDecimal;
            obj.AsDynamic().TETYPE2PLACECLASSTETYPECODE = t.GetKey();
            obj.AsDynamic().TETYPE2PLACECLASSPLACECLASSCODE = p.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TETYPE2PLACECLASSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(TEType2PlaceClass obj)
        {
            obj.AsDynamic().TETYPE2PLACECLASSDESC = TestString;
        }

        protected override void CheckSimpleChange(TEType2PlaceClass source, TEType2PlaceClass dest)
        {
            string sourceName = source.AsDynamic().TETYPE2PLACECLASSDESC;
            string destName = dest.AsDynamic().TETYPE2PLACECLASSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _teTypeTest.ClearForSelf();
            _placeClassTest.ClearForSelf();
        }
    }
}