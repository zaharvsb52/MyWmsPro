namespace wmsMLC.Tests.Functional.Entities
{

//TODO: Перенести в TECH-тесты
//    [TestFixture(Description = "Данный тест проверяет работу механизма транзакций на уровне DAL.Oracel")]
//    public class TransactionTest
//    {
//        [TestFixtureSetUp]
//        public void Setup()
//        {
//            BLHelper.InitBL(dalType: DALType.Oracle);
//        }
//
//        [Test]
//        public void TestTransaction()
//        {
//            const string firstItemCode = "TransactionTestCode1";
//
//            var mgr = IoC.Instance.Resolve<IBaseManager<AreaType>>();
//            var areaType1 = new AreaType();
//            areaType1.AsDynamic().AREATYPECODE = firstItemCode;
//            areaType1.AsDynamic().AREATYPENAME = "TransactionTestName1";
//
//            var areaType2 = new AreaType();
//            // такой длинный точно не вставится
//            areaType2.AsDynamic().AREATYPECODE = "TransactionTestCode22222222222222222222222222222222222222222222222222222222222222222222222";
//            areaType2.AsDynamic().AREATYPENAME = "TransactionTestName2";
//
//            var items = new List<AreaType>();
//            items.Add(areaType1);
//            items.Add(areaType2);
//
//            bool except = false;
//
//            try
//            {
//                var param = (IEnumerable<AreaType>)items;
//                mgr.Insert(ref param);
//            }
//            catch (Exception ex)
//            {
//                except = true;
//            }
//
//            except.Should().BeTrue("При вставке должено было возникнуть исключение");
//            mgr.ClearCache();
//            var areaTypeRes = mgr.Get(firstItemCode);
//            areaTypeRes.Should().BeNull("Первый объект не должен был вставиться");
//        }
//    }
}