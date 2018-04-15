using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace MLC.Wms.Tests.Integration.Entities
{
    public abstract class BaseEntityTest<TEntity>
        where TEntity : WMSBusinessObject
    {
        #region .  Properties  .

        protected const int MaxTimeDiffWithServerInSeconds = 5;

        protected const string CurrentUser = "TECH_AUTOTEST";
        protected const string TstMandantCode = "TST";
        protected const decimal TstMandantId = 5002;

        protected Lazy<PropertyDescriptorCollection> Properties =
            new Lazy<PropertyDescriptorCollection>(() => TypeDescriptor.GetProperties(typeof (TEntity)));

        protected StringBuilder Log = new StringBuilder();

        protected bool HaveHistory { get; set; }
        protected string SimpleChangePropertyName { get; set; }

        protected decimal InsertItemDecimalId { get; set; }
        protected string InsertItemStringId { get; set; }
        protected Guid InsertItemGuidId { get; set; }
        protected int InsertItemTransact { get; set; }
        protected int UpdateItemTransact { get; set; }

        protected bool TestBool { get; set; }
        protected decimal TestDecimal { get; set; }
        protected double TestDouble { get; set; }
        protected string TestString { get; set; }
        protected Guid TestGuid { get; set; }
        protected DateTime TestDateTime { get; set; }

        protected TimeSpan MaxTimeForGetByPk { get; set; }
        protected TimeSpan MaxTimeForGetByFilter { get; set; }
        protected TimeSpan MaxTimeForDelete { get; set; }
        protected TimeSpan MaxTimeForUpdate { get; set; }
        protected TimeSpan MaxTimeForInsert { get; set; }

        #endregion

        #region .  Test methods  .

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            HaveHistory = true;

            TestDouble = 1.0;
            TestDecimal = 123;
            TestString = "AUTOTEST" + typeof (TEntity).Name;
            TestGuid = new Guid();
            TestDateTime = DateTime.Now;
            TestDateTime = new DateTime(TestDateTime.Year, TestDateTime.Month, TestDateTime.Day, TestDateTime.Hour,
                TestDateTime.Minute, TestDateTime.Second);

            InsertItemStringId = "AUTOTEST";
            InsertItemDecimalId = -10;
            InsertItemGuidId = new Guid("00000000-0000-0000-0000-000000000010");
            InsertItemTransact = 1;
            UpdateItemTransact = 2;

            MaxTimeForGetByPk = 500.Milliseconds();
            MaxTimeForGetByFilter = 500.Milliseconds();
            MaxTimeForDelete = 500.Milliseconds();
            MaxTimeForUpdate = 500.Milliseconds();
            MaxTimeForInsert = 500.Milliseconds();
        }

        [TestFixtureTearDown]
        public virtual void TestTrearDown()
        {
            // переносим в общий лог
            IntegrationTestsSetUpClass.Log.Append(Log);
        }

        [Test]
        public virtual void Entity_should_be_create_read_update_delete()
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();

            using (var uow = uowFactory.Create())
            using (var mgr = CreateManager())
            {
                mgr.SetUnitOfWork(uow);
                uow.BeginChanges();

                try
                {
                    // создаем новый экземпляр
                    var obj = mgr.New();
                    obj.Should().NotBeNull("Manager должен уметь создавать новый экземпляр объекта");

                    // создаем все нужные для создания объекты и
                    // заполняем обязательные поля
                    FillRequiredFields(obj);

                    // сохраняем в БД
                    DoWithStopwatch(() => mgr.Insert(ref obj), "Insert", MaxTimeForInsert);

                    var insCheckDate = DateTime.Now;
                    // убеждаемся, что ключ заполнился
                    var key = ((IKeyHandler) obj).GetKey();
                    key.Should().NotBeNull("У сохраненного объекта Ключ должен быть заполнен");

                    // проверяем служебные поля
                    CheckSysFields(obj, InsertItemTransact, insCheckDate);

                    // Обновляем
                    if (MakeSimpleChange(obj))
                    {
                        DoWithStopwatch(() => mgr.Update(obj), "Update", MaxTimeForUpdate);
                        var updCheckDate = DateTime.Now;
                        // NOTE: по-хорошему тут не нужно ничего чистить, обновление должно само инициировать очистку (нужно будет поправить - пока не делаю, т.к. не хочу лезть в core до перехода на железо)
                        mgr.ClearCache();

                        // читаем из БД по ключу
                        var updated = DoWithStopwatch(() => mgr.Get(key), "Get", MaxTimeForGetByPk);
                        updated.Should().NotBeNull("Мы должны уметь получать объект по ключу");

                        // сравниваем ключи
                        var insKey = ((IKeyHandler) updated).GetKey();
                        insKey.Should()
                            .NotBeNull("Ключ полученного объект должен соответствовать ключу переданного")
                            .And.Be(key, "Ключи должны совпадать");

                        // проверяем, что изменения прошли
                        CheckSimpleChange(obj, updated);

                        CheckSysFields(updated, UpdateItemTransact, updCheckDate);

                        // todo: other simple change (for locking check)
                    }

                    // удаляем
                    DoWithStopwatch(() => mgr.Delete(obj), "Delete", MaxTimeForDelete);

                    // сущность может быть кэшируемой
                    mgr.ClearCache();

                    // убеждаемся, что корректно удалили
                    var deleted = mgr.Get(key);
                    deleted.Should().BeNull("Удаленный объект нельзя получить из БД");
                }
                finally
                {
                    // откатываем какие-либо изменения (чтобы не мусорить)
                    uow.RollbackChanges();
                }
            }
        }

        [Test]
        public virtual void Filter_should_return_empty_collections()
        {
            using (var mgr = CreateManager())
            {
                var items = mgr.GetFiltered("1=2");
                items.Should().BeEmpty("Фиктивный фильтр не должен работать");
            }
        }

        [Test]
        public virtual void Filter_should_return_non_empty_collections()
        {
            using (var mgr = CreateManager())
            {
                // здаем нормальный фильтр
                var items = DoWithStopwatch(() => mgr.GetFiltered("1=1 and ROWNUM < 2").ToArray(), "GetFiltered", MaxTimeForGetByFilter);

                items.Should().NotBeEmpty("По фильтру должны получить одну запись. Проверьте формирование фильтра или наличие данных.");
                items.Should().HaveCount(1);
            }
        }

        [Test]
        public virtual void Filter_should_work_by_all_entity_fields()
        {
            // формируем фильтр по всем атрибутам
            var defFilter = string.Empty;
            var defInternalFilter = string.Empty;
            foreach (PropertyDescriptor p in Properties.Value)
            {
                var isList = typeof (IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof (string);
                var isObject = typeof (WMSBusinessObject).IsAssignableFrom(p.PropertyType);
                var dbField = SourceNameHelper.Instance.GetPropertySourceNameWithNoVirtualFields(typeof (TEntity),
                    p.Name);

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

            // не должен упасть
        }

        [Test]
        public virtual void Entity_should_have_history()
        {
            if (!HaveHistory)
                return;

            using (var mgr = CreateManager())
            {
                var hm = mgr as IHistoryManager<TEntity>;
                if (hm == null)
                    return;

                // главное, чтобы запустилось
                hm.GetHistory("1=2");
            }
        }

        [Test]
        public virtual void Entity_declared_pk_type_should_be_eq_db_pk_type()
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

            var property = Properties.Value[propertyName];
            var proprtyType = property.PropertyType.GetNonNullableType();

            proprtyType.ShouldBeEquivalentTo(managerKeyType);
        }

        #endregion

        #region .  Helper methods  .
        protected virtual string GetSimpleFilter(IBaseManager<TEntity> mgr)
        {
            return "1=1 and ROWNUM < 2";
        }

        protected virtual IBaseManager<TEntity> CreateManager()
        {
            return IoC.Instance.Resolve<IBaseManager<TEntity>>();
        }

        protected virtual void CheckSimpleChange(TEntity entity, TEntity updated)
        {
            if (string.IsNullOrEmpty(SimpleChangePropertyName))
                throw new Exception("CheckSimpleChange can be called only if SimpleChangePropertyName is exists.");

            var fieldName = SimpleChangePropertyName.ToUpper();
            var property = Properties.Value[fieldName];
            if (property == null)
                throw new Exception(string.Format("Не найдено свойство {0} для simple change.", fieldName));

            var updatedValue = property.GetValue(updated);
            var sourceValue = GetTestValueByType(property.PropertyType);

            updatedValue.ShouldBeEquivalentTo(sourceValue);
        }

        protected virtual bool MakeSimpleChange(TEntity entity)
        {
            if (string.IsNullOrEmpty(SimpleChangePropertyName))
                return false;

            var fieldName = SimpleChangePropertyName.ToUpper();
            var property = Properties.Value[fieldName];
            if (property == null)
                throw new Exception(string.Format("Не найдено свойство {0} для simple change.", fieldName));
            property.SetValue(entity, GetTestValueByType(property.PropertyType));

            return true;
        }

        protected virtual object GetTestValueByType(Type type)
        {
            var notNullableType = type.GetNonNullableType();
            if (notNullableType == typeof(bool))
                return TestBool;
            if (notNullableType == typeof(decimal))
                return TestDecimal;
            if (notNullableType == typeof(double))
                return TestDouble + 1;
            if (notNullableType == typeof(string))
                return TestString;
            if (notNullableType == typeof(Guid))
                return TestGuid;
            if (notNullableType == typeof(DateTime))
                return TestDateTime;

            throw new Exception(string.Format("Тествое значение по умолчанию для типа {0} не определено.", type));
        }

        protected virtual void FillRequiredFields(TEntity entity)
        {
            // pk
            var pkPropertyName = entity.GetPrimaryKeyPropertyName();
            var pkProperty = Properties.Value[pkPropertyName];

            var notNullablePropertyType = pkProperty.PropertyType.GetNonNullableType();
            if (notNullablePropertyType == typeof(decimal))
                entity.SetKey(InsertItemDecimalId);
            else if (notNullablePropertyType == typeof(string))
                entity.SetKey(InsertItemStringId);
            else if (notNullablePropertyType == typeof(Guid))
                entity.SetKey(InsertItemGuidId);
            else
                throw new NotImplementedException(string.Format("Автоматическое задание PK с типом {0} не поддерживается", notNullablePropertyType));
        }

        protected virtual void CheckSysFields(TEntity obj, int alreadySavingCount, DateTime checkDate)
        {
            if (alreadySavingCount == 1)
            {
                obj.UserIns.ShouldBeEquivalentTo(CurrentUser, "Создание должно проходить от пользователя " + CurrentUser);
                obj.DateIns.Should().BeAfter(checkDate.AddSeconds(-1 * MaxTimeDiffWithServerInSeconds),
                    string.Format(
                        "Дата создания должна соответствовать дате команды. Делаем допущение в {0} секунд на случай расхождения времени на сервере и клиенте.",
                        MaxTimeDiffWithServerInSeconds));
                obj.UserUpd.Should().Be(null, "UserUpd проставляется только после обновления");
                obj.DateUpd.Should().Be(null, "DateUpd проставляется только после обновления");
                obj.Transact.Should().Be(1, "После создания Transact д.б. 1");
            }
            else
            {
                obj.UserIns.Should().NotBeNullOrEmpty();
                obj.DateIns.Should().HaveValue();
                obj.UserUpd.ShouldBeEquivalentTo(CurrentUser, "Обновление должно проходить от пользователя " + CurrentUser);
                obj.DateUpd.Should().BeAfter(checkDate.AddSeconds(-1 * MaxTimeDiffWithServerInSeconds),
                    string.Format(
                        "Дата обновления должна соответствовать дате команды. Делаем допущение в {0} секунд на случай расхождения времени на сервере и клиенте.",
                        MaxTimeDiffWithServerInSeconds));
                obj.Transact.ShouldBeEquivalentTo(alreadySavingCount);
            }
        }

        private static Type GetWMSBusinessObjectManagerType(Type nowType)
        {
            if (nowType == null)
                return null;

            if (nowType.IsGenericType && 
                nowType.GetGenericTypeDefinition() == typeof(BusinessObjectManager<,>))
                return nowType;

            if (nowType.BaseType == null)
                return null;

            return GetWMSBusinessObjectManagerType(nowType.BaseType);
        }

        private void DoWithStopwatch(Action action, string operation, TimeSpan? maxTimeout)
        {
            var sw = new Stopwatch();

            sw.Start();
            action.Invoke();
            sw.Stop();

            if (!string.IsNullOrEmpty(operation))
                LogOperation(operation, sw.Elapsed);

            //if (maxTimeout.HasValue)
            //    sw.Elapsed.Should().BeLessThan(maxTimeout.Value);
        }

        private TRes DoWithStopwatch<TRes>(Func<TRes> action, string operation, TimeSpan? maxTimeout)
        {
            var sw = new Stopwatch();

            sw.Start();
            var res = action.Invoke();
            sw.Stop();

            if (!string.IsNullOrEmpty(operation))
                LogOperation(operation, sw.Elapsed);

            //if (maxTimeout.HasValue)
            //    sw.Elapsed.Should().BeLessThan(maxTimeout.Value);

            return res;
        }

        private void LogOperation(string operation, TimeSpan time)
        {
            Log.AppendLine(string.Format("{0};{1};{2};{3}", DateTime.Now, typeof(TEntity).Name, operation, time));
        }
        #endregion
    }
}