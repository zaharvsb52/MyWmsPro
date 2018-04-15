using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TEType2TruckTypeTest : BaseWMSObjectTest<TEType2TruckType>
    {
        private readonly TETypeTest _teTypeTest = new TETypeTest();
        private readonly TruckTypeTest _truckTypeTest = new TruckTypeTest();

        protected override void FillRequiredFields(TEType2TruckType obj)
        {
            base.FillRequiredFields(obj);

            var te = _teTypeTest.CreateNew();
            var tr = _truckTypeTest.CreateNew();

            obj.AsDynamic().TETYPE2TRUCKTYPEID = TestDecimal;
            obj.AsDynamic().TETYPE2TRUCKTYPECOUNT = TestDecimal;
            obj.AsDynamic().TETYPE2TRUCKTYPETETYPECODE = te.GetKey();
            obj.AsDynamic().TETYPE2TRUCKTYPETRUCKTYPECODE = tr.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TETYPE2TRUCKTYPEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(TEType2TruckType obj)
        {
            obj.AsDynamic().TETYPE2TRUCKTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(TEType2TruckType source, TEType2TruckType dest)
        {
            string sourceName = source.AsDynamic().TETYPE2TRUCKTYPEDESC;
            string destName = dest.AsDynamic().TETYPE2TRUCKTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _truckTypeTest, _teTypeTest, };
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<TEType>(TestDecimal, TestString);
            DeleteByParent<TruckType>(TestDecimal, TestString);
        }
    }
}