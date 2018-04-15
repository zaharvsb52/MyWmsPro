using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class AreaTypeGPVTest : BaseWMSObjectTest<GlobalParamValue>
    {
        private const string _areaTypeName = "AreaType";

        private readonly AreaTypeTest _areaTypeTest = new AreaTypeTest();
        private readonly GlobalParamTest _globalParamTest = new GlobalParamTest();

        public AreaTypeGPVTest()
        {
            TestString = "AutoTestAreaTypeGPV";
            _areaTypeTest.TestString = TestString;
            _globalParamTest.TestString = TestString;
        }

        protected override void FillRequiredFields(GlobalParamValue obj)
        {
            base.FillRequiredFields(obj);

            var gp = _globalParamTest.CreateNew(param =>
                {
                    param.AsDynamic().GLOBALPARAMLOCKED = 0;
                    param.AsDynamic().GLOBALPARAMDATATYPE = 6;
                    param.AsDynamic().GLOBALPARAMCOUNT = 1;
                    param.AsDynamic().GLOBALPARAM2ENTITY = _areaTypeName;
                });

            var area = _areaTypeTest.CreateNew();

            obj.AsDynamic().GPARAMID = TestDecimal;
            obj.AsDynamic().GPARAMVALKEY = TestString;
            obj.AsDynamic().GLOBALPARAMCODE_R = gp.GetKey();
            obj.AsDynamic().GPARAMVAL2ENTITY = area.GetKey();
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
            _areaTypeTest.ClearForSelf();
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}