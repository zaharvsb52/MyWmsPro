using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SegmentTypeGPVTest : BaseWMSObjectTest<GlobalParamValue>
    {
        private const string _segmentTypeName = "SegmentTypeName";

        private readonly SegmentTypeTest _segmentTypeTest = new SegmentTypeTest();
        private readonly GlobalParamTest _globalParamTest = new GlobalParamTest();

        public SegmentTypeGPVTest()
        {
            TestString = "AutoTestSegmentTypeGPV";
            _segmentTypeTest.TestString = TestString;
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
                param.AsDynamic().GLOBALPARAM2ENTITY = _segmentTypeName;
            });
            
            var s = _segmentTypeTest.CreateNew();

            obj.AsDynamic().GPARAMVALKEY = TestString;
            obj.AsDynamic().GLOBALPARAMCODE_R = gp.GetKey();
            obj.AsDynamic().GPARAMVAL2ENTITY = s.GetKey();
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
            _segmentTypeTest.ClearForSelf();
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}

