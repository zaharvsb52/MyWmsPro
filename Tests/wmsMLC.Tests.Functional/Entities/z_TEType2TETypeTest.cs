using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TEType2TETypeTest : BaseWMSObjectTest<TEType2TEType>
    {
        private readonly TETypeTest _teTypeTestSlave = new TETypeTest();
        private readonly TETypeTest _teTypeTestMaster = new TETypeTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _teTypeTestSlave, _teTypeTestMaster };
        }

        public TEType2TETypeTest()
        {
            _teTypeTestSlave.TestString = TestString + "1";
            _teTypeTestMaster.TestString = TestString + "2";
        }

        protected override void FillRequiredFields(TEType2TEType obj)
        {
            base.FillRequiredFields(obj);

            var teTypeSlave = _teTypeTestSlave.CreateNew();
            var teTypeMaster = _teTypeTestMaster.CreateNew();
            
            obj.AsDynamic().TETYPE2TETYPEID = TestDecimal;
            obj.AsDynamic().TETYPE2TETYPESLAVE = teTypeSlave.GetKey();
            obj.AsDynamic().TETYPE2TETYPEMASTER = teTypeMaster.GetKey();
            obj.AsDynamic().TETYPE2TETYPECAPACITY = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TETYPE2TETYPEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(TEType2TEType obj)
        {
            obj.AsDynamic().TETYPE2TETYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(TEType2TEType source, TEType2TEType dest)
        {
            string sourceName = source.AsDynamic().TETYPE2TETYPEDESC;
            string destName = dest.AsDynamic().TETYPE2TETYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}