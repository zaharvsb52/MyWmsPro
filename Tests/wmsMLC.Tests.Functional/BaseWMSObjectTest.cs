using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional
{
    public abstract class BaseWMSTest
    {
        #region .  Fields & consts  .
        public const string AutoTestMagicWord = "AutoTest";
        public const string DeleteByParentDesc = "Вложеная сущность должна удаляться при удалении основной";

        private string _testString;
        private decimal _testDecimal;
        private double _testDouble;
        private Guid _testGuid;
        private static bool _initialized;
        #endregion

        #region .  Properties  .
        public virtual string TestString
        {
            get { return _testString; }
            set
            {
                _testString = value;

                foreach (var test in GetDependencyTests())
                    test.TestString = value;
            }
        }

        public virtual decimal TestDecimal
        {
            get { return _testDecimal; }
            set
            {
                _testDecimal = value;

                foreach (var test in GetDependencyTests())
                    test.TestDecimal = value;
            }
        }

        public double TestDouble
        {
            get { return _testDouble; }
            set
            {
                _testDouble = value;

                foreach (var test in GetDependencyTests())
                    test.TestDouble = value;
            }
        }

        public virtual Guid TestGuid
        {
            get { return _testGuid; }
            set
            {
                _testGuid = value;

                foreach (var test in GetDependencyTests())
                    test._testGuid = value;
            }
        }
        #endregion

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            if (_initialized)
                return;

            //ConfigurationManager.AppSettings["BLToolkit.DefaultConfiguration"] = "DEV";

            IAuthenticationProvider auth;
            if (!IoC.Instance.TryResolve<IAuthenticationProvider>(out auth))
                BLHelper.InitBL(dalType: DALType.Oracle);

            //BLHelper.InitBL(dalType: DALType.Service);
            //BLHelper.RegisterServiceClient("Auto", ClientTypeCode.DCL, Properties.Settings.Default.SDCL_Endpoint);

            if (auth == null)
                auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");

            //BLHelper.FillInitialCaches();

            _initialized = true;
        }

        public virtual void ClearForSelf()
        {
            // чистим все зависимые тесты
            var dependencyTests = GetDependencyTests();
            foreach (var test in dependencyTests)
                test.ClearForSelf();
        }

        protected virtual IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[0];
        }
    }

    public abstract class BaseWMSObjectTest<T> : BaseWMSTest
        where T : class, new()
    {
        protected BaseWMSObjectTest()
        {
            TestDouble = 1.0;
            TestDecimal = 123;
            TestString = AutoTestMagicWord + typeof(T).Name;
            TestGuid = new Guid();
        }

        #region .  Settings  .

        /// <summary>
        /// Метод, который будет вызван при любом окончании 
        /// <remarks>Гарантированно чистим за собой</remarks>
        /// </summary>
        [TestFixtureTearDown]
        public virtual void TearDown()
        {
            ClearForSelf();
        }

        [SetUp]
        public virtual void SetupForEach()
        {
            // перед выполнением каждого теста чистим за собой
            ClearForSelf();
        }
        #endregion

        [Test(Description = "Manager должен уметь делать CRUD")]
        public virtual void ManagerCRUDTest()
        {
            var mgr = CreateManager();

            // создаем новый экземпляр
            var obj = mgr.New();
            obj.Should().NotBeNull("Manager должен уметь создавать новый экземпляр объекта");

            // создаем все нужные для создания объекты и
            // заполняем обязательные поля
            FillRequiredFields(obj);

            // сохраняем в БД
            mgr.Insert(ref obj);

            // убеждаемся, что ключ заполнился
            var key = ((IKeyHandler)obj).GetKey();
            key.Should().NotBeNull("У сохраненного объекта Ключ должен быть заполнен");

            // читаем из БД по ключу
            obj = mgr.Get(key);
            obj.Should().NotBeNull("Мы должны уметь получать объект по ключу");

            // сравниваем ключи
            var insKey = ((IKeyHandler)obj).GetKey();
            insKey.Should()
                .NotBeNull("Ключ полученного объект должен соответствовать ключу переданного")
                .And.Be(key, "Ключи должны совпадать");

            // Обновляем
            MakeSimpleChange(obj);
            mgr.Update(obj);

            // читаем из БД по ключу
            var updated = mgr.Get(key);
            updated.Should().NotBeNull("Мы должны уметь получать объект по ключу");

            // проверяем, что изменения прошли
            CheckSimpleChange(obj, updated);

            // удаляем из БД
            ClearForSelf();

            // убеждаемся, что корректно удалили
            var deleted = mgr.Get(key);
            deleted.Should().BeNull("Удаленный объект нельзя получить из БД");
        }

        [Test(Description = "Manager должен уметь получать все объекты")]
        [Ignore("После переноса данных с PRD - не актуальный")]
        public virtual void ManagerGetAllTest()
        {
            var mgr = CreateManager();

            // создаем новый экземпляр
            var obj = CreateNew();

            // вычитываем все данные
            var items = mgr.GetAll();
            items.Should().NotBeNull();
            items.Should().NotBeEmpty("Должны получить хотя бы один элемент");

            ClearForSelf();
        }

        [Test(Description = "Manager должен уметь получать объекты по фильтрам")]
        public virtual void ManagerGetFilteredTest()
        {
            var mgr = CreateManager();

            // задаем несуществующий фильтр
            var items = mgr.GetFiltered("1=2");
            items.Should().BeEmpty("Фиктивный фильтр не должен работать");

            var obj = CreateNew();

            // здаем нормальный фильтр
            var filter = GetSimpleFilter();
            items = mgr.GetFiltered(filter);
            items.Should().NotBeEmpty("По фильтру должны получить хотя бы одну запись. Проверьте формирование фильтра");

            ClearForSelf();
        }

        [Test]
        public virtual void ManagerGetHistoryTest()
        {
            var mgr = CreateManager();
            var hm = mgr as IHistoryManager<T>;
            if (hm == null)
                return;

            // главное, чтобы запустилось
            hm.GetHistory("1=2");
        }

        [Test(Description = "Тип первичного ключа должен совпадать с типом в базе")]
        public virtual void PKTypeTest()
        {
            var mgr = CreateManager();
            var item = mgr.New();
            var kh = item as IKeyHandler;
            if (kh == null)
                throw new Exception("Все объекты должны поддерживать работу с первичным ключом");

            var managerGenericType = GetWMSBusinessObjectManagerType(mgr.GetType());
            if (managerGenericType == null)
                throw new Exception("Не удалось обнаружить в цепочки наследования тип WMSBusinessObjectManager");
            var managerKeyType = managerGenericType.GetGenericArguments()[1];

            var propertyName = kh.GetPrimaryKeyPropertyName();
            if (string.IsNullOrEmpty(propertyName))
                throw new Exception("Не удалось обнаружить свойство, которое является первичным ключом");

            var properties = TypeDescriptor.GetProperties(item);
            var property = properties[propertyName];
            var proprtyType = property.PropertyType.GetNonNullableType();

            proprtyType.ShouldBeEquivalentTo(managerKeyType);
        }

        [Test(Description = "Фильтр должен работать по всем атрибутам сущности")]
        public virtual void FiltersTest()
        {
            // считываем атрибуты
            var properties = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>();
            
            // формируем фильтр по всем атрибутам
            var defFilter = string.Empty;
            var defInternalFilter = string.Empty;
            foreach (var p in properties)
            {
                var isList = typeof(IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof(string);
                var isObject = typeof(WMSBusinessObject).IsAssignableFrom(p.PropertyType);
                var dbField = SourceNameHelper.Instance.GetPropertySourceNameWithNoVirtualFields(typeof(T), p.Name);

                // если атрибут сущности = список вложенных сущностей
                if (isList || isObject)
                {
                    // TODO: фильтр по атрибутам вложенных сущностей временно отключен! Ждём Надю
//                    var itemType = p.PropertyType;
//                    if (isList)
//                    {
//                        var itemCollectionType = p.PropertyType.GetGeneticTypeFormInheritanceNode(typeof(BusinessObjectCollection<>));
//                        if (itemCollectionType == null)
//                            continue;
//                        itemType = itemCollectionType.GetGenericArguments()[0];
//                    }
//
//                    // формируем тип и список атрибутов вложенной сущности
//                    var listInternalFields = TypeDescriptor.GetProperties(itemType).Cast<PropertyDescriptor>();
//                    foreach (var internalField in listInternalFields)
//                    {
//                        var dbInternalField = SourceNameHelper.Instance.GetPropertySourceName(itemType, internalField.Name);
//
//                        var defVal = "0";
//                        if (internalField.PropertyType.IsAssignableFrom(typeof(string)))
//                            defVal = "'0'";
//                        if (internalField.PropertyType.IsAssignableFrom(typeof(DateTime)))
//                            defVal = "'00010101 00:00:00'";
//
//                        if (!string.IsNullOrEmpty(defInternalFilter))
//                            defInternalFilter += " AND ";
//                        defInternalFilter += string.Format("{0}.{1} = {2}", dbField, dbInternalField, defVal);
//                    }
                }
                else
                {
                    if (dbField != null)
                    {
                        if (!string.IsNullOrEmpty(defFilter))
                            defFilter += " AND ";
                        defFilter += string.Format("{0} is null", dbField);
                    }
                }
            }
            // TODO: фильтр по атрибутам вложенных сущностей временно отключен! Ждём Надю
            //defFilter += defInternalFilter;

            // пытаемся применить фильтр
            var objMgr = CreateManager();
            objMgr.GetFiltered(defFilter);
            ClearForSelf();
        }

        [Test(Description = "Запрет параллельного CRUD'a сущности")]
        public virtual void ParallelCRUDTest()
        {
            var objMgr = CreateManager();
            var obj1 = CreateNew();
            objMgr.Update(obj1);
            try
            {
                MakeSimpleChange(obj1);
                objMgr.Update(obj1);
            }
            catch (Exception ex)
            {
                ex.InnerException.Message.Should().Contain("Объект изменен пользователем");
            }
            finally
            {
                ClearForSelf();
            }
        }
/*
        [Test(Description = "PK объявленый в коде должен совпадать с SysObj")]
        public virtual void PKTest()
        {
            var item = new T() as WMSBusinessObject;
            if (item == null)
                throw new DeveloperException("Пытаемся получить PK не от WMSBusinessObject");

            // вычисляем тип из SysObject
            var pkPropertyName = item.GetPrimaryKeyPropertyName();
            var pkProperty = TypeDescriptor.GetProperties(typeof (T))[pkPropertyName];
            var nonNullablePkType = pkProperty.PropertyType.GetNonNullableType();

            // получаем тип, с которым зарегистрировали
            var registration = BLHelper.Registered.First(i => i.ObjectType == typeof (T));
            var regType = registration.KeyType.GetNonNullableType();

            // сверяем
            regType.Should().Be(nonNullablePkType, "Тип PK из SysObject должен соответсвовать зарегистрированному");
        }
*/
        public virtual void DeleteByParent<TParent>(object childKey, object parentKey)
        {
            if (childKey == null)
                throw new ArgumentNullException("childKey");
            if (parentKey == null)
                throw new ArgumentNullException("parentKey");

            // создаем manager'ов
            var childMgr = CreateManager();
            var parentMgr = CreateManager<TParent>();

            try
            {
                // создаём инстансы, запоминаем ключи
                CreateNew();
                var parent = parentMgr.Get(parentKey);

                // удаляем родителя
                parentMgr.Delete(parent);

                // убеждаемся, что корректно удалили
                var child = childMgr.Get(childKey);
                child.Should().BeNull("При удалении {0} должны быть удалены все его вложенные сущности",parent);
            }
            finally
            {
                ClearForSelf();
            }
        }

        private static Type GetWMSBusinessObjectManagerType(Type nowType)
        {
            if (nowType == null)
                return null;

            if (nowType.IsGenericType && nowType.GetGenericTypeDefinition() == typeof(WMSBusinessObjectManager<,>))
                return nowType;

            if (nowType.BaseType == null)
                return null;

            return GetWMSBusinessObjectManagerType(nowType.BaseType);
        }

        public override void ClearForSelf()
        {
            // получаем все наделанные объекты
            var filter = GetCheckFilter();
            if (filter == null)
                return;

            // удаляем
            var mgr = CreateManager();
            var obj = mgr.GetFiltered(filter);
            if (obj != null)
                foreach (var o in obj)
                    ClearForSelf(o);

            base.ClearForSelf();
        }

        [Obsolete]
        public virtual void ClearForSelf(T obj)
        {
            var mgr = CreateManager();
            mgr.Delete(obj);
        }

        [Obsolete]
        public virtual void ClearForSelfByKey(object key)
        {
            var mgr = CreateManager();
            var obj = mgr.Get(key);
            ClearForSelf(obj);
        }

        public virtual T CreateNew(Action<T> customFill = null)
        {
            //ClearForSelf();

            // создаем новый экземпляр
            var mgr = CreateManager();
            var obj = mgr.New();
            FillRequiredFields(obj);
            if (customFill != null)
                customFill(obj);
            mgr.Insert(ref obj);
            return obj;

        }

        protected virtual string GetCheckFilter()
        {
            // фильтр будет пустым
            return null;
        }
        protected virtual string GetSimpleFilter()
        {
            // получить все записи
            return "1=1 and ROWNUM < 2";
        }
        protected virtual void FillRequiredFields(T obj) { }
        protected virtual void MakeSimpleChange(T obj) { }
        protected virtual void CheckSimpleChange(T source, T dest) { }

        public virtual IBaseManager<T> CreateManager()
        {
            return IoC.Instance.Resolve<IBaseManager<T>>();
        }
        public virtual IBaseManager<TObj> CreateManager<TObj>()
        {
            return IoC.Instance.Resolve<IBaseManager<TObj>>();
        }
    }
}