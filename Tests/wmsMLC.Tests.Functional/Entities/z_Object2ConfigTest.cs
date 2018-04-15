using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Object2ConfigTest : BaseWMSObjectTest<Object2Config>
    {
        private readonly ObjectConfigTest _objectConfigTest = new ObjectConfigTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _objectConfigTest };
        }

        protected override void FillRequiredFields(Object2Config obj)
        {
            base.FillRequiredFields(obj);

            var oc = _objectConfigTest.CreateNew();
            var mgr = IoC.Instance.Resolve<ISysObjectManager>();

            obj.AsDynamic().OBJECT2CONFIGID = TestDecimal;
            obj.AsDynamic().OBJECT2CONFIGOBJECTCONFIGCODE = oc.GetKey();
            obj.AsDynamic().OBJECT2CONFIGOBJECTNAME = mgr.Get(0).GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OBJECT2CONFIGID = '{0}')", TestDecimal);
        }
    }
}