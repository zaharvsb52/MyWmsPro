//using FluentAssertions;
//using NUnit.Framework;
//using wmsMLC.Business.Objects;

//namespace wmsMLC.Tests.Functional
//{
//    [TestFixture]
//    class EpsConfigTest : BaseWMSObjectTest<EpsConfig>
//    {
//        public const string TestName = "AutoTestEpsConfig";
//        public const bool TestBool = false;

//        protected override string GetCheckFilter()
//        {
//            return string.Format("((EPSCONFIG2ENTITY='{0}' OR EPSCONFIGKEY='{0}' OR EPSCONFIGPARAMCODE='{0}' ))", TestName);
//        }

//        protected override void FillRequiredFields(EpsConfig obj)
//        {
//            base.FillRequiredFields(obj);
//            obj.AsDynamic().EPSCONFIG2ENTITY = TestName;
//            obj.AsDynamic().EPSCONFIGKEY = TestName;
//            obj.AsDynamic().EPSCONFIGPARAMCODE = TestName;
//            obj.AsDynamic().EPSCONFIGSTRONGUSE = TestBool;
//            obj.AsDynamic().EPSCONFIGLOCKED = TestBool;
//        }

//        protected override void MakeSimpleChange(EpsConfig obj)
//        {
//            obj.AsDynamic().EPSCONFIGDESC = TestName;
//        }

//        protected override void CheckSimpleChange(EpsConfig source, EpsConfig dest)
//        {
//            string sourceName = source.AsDynamic().EPSCONFIGDESC;
//            string destName = dest.AsDynamic().EPSCONFIGDESC;
//            sourceName.ShouldBeEquivalentTo(destName);
//        }
//    }
//}
