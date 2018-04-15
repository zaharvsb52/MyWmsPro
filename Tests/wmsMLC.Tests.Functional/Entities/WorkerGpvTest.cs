using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WorkerGpvTest : BaseWMSObjectTest<GlobalParamValue>
    {
        private readonly string WorkerName = "WORKER";

        private readonly GlobalParamTest _globalParamTest = new GlobalParamTest();
        private readonly WorkerTest _workerTest = new WorkerTest();

        public WorkerGpvTest()
        {
            TestString = "AutoTestWorkerGpv";
            _globalParamTest.TestString = TestString;
            _workerTest.TestString = TestString;
        }
        
        protected override void FillRequiredFields(GlobalParamValue obj)
        {
            base.FillRequiredFields(obj);

            var gp = _globalParamTest.CreateNew(param =>
                {
                    param.AsDynamic().GLOBALPARAMLOCKED = 0;
                    param.AsDynamic().GLOBALPARAMDATATYPE = 6;
                    param.AsDynamic().GLOBALPARAMCOUNT = 1;
                    param.AsDynamic().GLOBALPARAM2ENTITY = WorkerName;
                });

            var w = _workerTest.CreateNew();

            obj.AsDynamic().GPARAMID = TestDecimal;
            obj.AsDynamic().GPARAMVALKEY = TestString;
            obj.AsDynamic().GLOBALPARAMCODE_R = gp.GetKey();
            obj.AsDynamic().GPARAMVAL2ENTITY = w.GetKey();
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
            _workerTest.ClearForSelf();
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }

}