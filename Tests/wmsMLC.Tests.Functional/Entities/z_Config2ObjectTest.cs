using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Config2ObjectTest : BaseWMSObjectTest<Config2Object>
    {
        private readonly ObjectConfigTest _objectConfigTest = new ObjectConfigTest();

        protected override void FillRequiredFields(Config2Object obj)
        {
            base.FillRequiredFields(obj);

            var oc = _objectConfigTest.CreateNew();
            var mgr = IoC.Instance.Resolve<ISysObjectManager>();

            obj.AsDynamic().CONFIG2OBJECTID = TestDecimal;
            obj.AsDynamic().CONFIG2OBJECTOBJECTCONFIGCODE = oc.GetKey();
            obj.AsDynamic().CONFIG2OBJECTOBJECTENTITYCODE = mgr.Get(0).GetKey();
            obj.AsDynamic().CONFIG2OBJECTOBJECTNAME = mgr.Get(0).GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CONFIG2OBJECTID = '{0}')", TestDecimal);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _objectConfigTest };
        }

        [Test(Description = DeleteByParentDesc),Ignore("Пока не работает удаление вложенных сущностей")]
        public void DeleteByParentTest()
        {
            DeleteByParent<ObjectConfig>(TestDecimal, TestString);
        }
    }
}