using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TETypeGPVTest : BaseWMSObjectTest<GlobalParamValue>
    {
        private const string TETypeName = "TETYPE";

        private readonly TETypeTest _teTypeTest = new TETypeTest();
        private readonly GlobalParamTest _globalParamTest = new GlobalParamTest();

        public TETypeGPVTest()
        {
            TestString = "AutoTestTETypeGPV";
            _teTypeTest.TestString = TestString;
            _globalParamTest.TestString = TestString;
        }
        protected override void FillRequiredFields(GlobalParamValue obj)
        {
            base.FillRequiredFields(obj);
            
            var t = _teTypeTest.CreateNew();
            var gp = _globalParamTest.CreateNew(param =>
                {
                    param.AsDynamic().GLOBALPARAMLOCKED = 0;
                    param.AsDynamic().GLOBALPARAM2ENTITY = TETypeName;
                    param.AsDynamic().GLOBALPARAMDATATYPE = 16;
                    param.AsDynamic().GLOBALPARAMCOUNT = 1;
                });
            
            obj.AsDynamic().GPARAMID = TestDecimal;
            obj.AsDynamic().GPARAMVALKEY = TestString;
            obj.AsDynamic().GLOBALPARAMCODE_R = gp.GetKey();
            obj.AsDynamic().GPARAMVAL2ENTITY = t.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(GPARAMID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(GlobalParamValue obj)
        {
            obj.AsDynamic().GPARAMVALVALUE = TestString;
        }

        protected override void CheckSimpleChange(GlobalParamValue source, GlobalParamValue dest)
        {
            string sourceName = source.AsDynamic().GPARAMVALVALUE;
            string destName = dest.AsDynamic().GPARAMVALVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _globalParamTest.ClearForSelf();
            _teTypeTest.ClearForSelf();
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}
