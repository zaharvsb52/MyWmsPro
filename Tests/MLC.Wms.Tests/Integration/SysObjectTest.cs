using NUnit.Framework;
using wmsMLC.Business.Managers;

namespace wmsMLC.Business.Tests
{
//    [TestFixture]
//    public class SysObjectTest
//    {
//        [SetUp]
//        public void Setup()
//        {
//            BLHelper.InitBL();
//        }
//
//        [Test]
//        public void ManagerGetObjectTest()
//        {
//            var mgr = new SysObjectManager();
//            // получаем тестовый объект
//            var item = mgr.Get(1129);
//            // проверяем, что все статические свойства нормально мапятся на динамический контейнер
//            var props = item.GetType().GetProperties();
//            foreach (var propertyInfo in props)
//            {
//                var val = propertyInfo.GetValue(item, null);
//            }
//        }
//
//        [Test]
//        public void ManagerGetAllTest()
//        {
//            var mgr = new SysObjectManager();
//            var items = mgr.GetAll();
//        }
//
//        [Test]
//        public void ManagerGetEntityTest()
//        {
//            var mgr = new SysObjectManager();
//            //var entity = mgr.GetEntity("PARTNER");
//            //entity = mgr.GetEntity("Partner");
//        }
//    }
}