using System;
using System.Collections;
using System.ComponentModel;
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
    public abstract class NewBaseWMSTest
    {
        #region .  Fields & consts  .
        public const string AutoTestMagicWord = "AutoTest";
        public const string AutoTestAnchorMagicWord = "AutoTestAnchor";

        private string _testString;
        private decimal _testDecimal;
        private double _testDouble;
        private static bool _initialized;

        #endregion

        #region .  Properties  .
        
        public virtual decimal TestDecimal
        {
            get
            {
                return IsAnchor ? TestDecimalAnchor : _testDecimal;
            }
            set
            {
                _testDecimal = value;
            }
        }
        public virtual string TestString
        {
            get
            {
                return IsAnchor ? TestStringAnchor : _testString;
            }
            set
            {
                _testString = value;
            }
        }
        public virtual double TestDouble
        {
            get
            {
                return IsAnchor ? TestDoubleAnchor : _testDouble;
            }
            set
            {
                _testDouble = value;
            }
        }

        public virtual string TestStringAnchor { get; set; }
        public virtual decimal TestDecimalAnchor { get; set; }
        public virtual double TestDoubleAnchor { get; set; }
        public virtual bool IsAnchor { get; set; }
        public virtual Guid TestGuid { get; set; }
    
        #endregion

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            if (_initialized)
                return;

            _initialized = true;
            BLHelper.InitBL(dalType: DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
        }

        public virtual void ClearForSelf()
        {
           
        }
    }


    public abstract class NewBaseWMSObjectTest<T> : NewBaseWMSTest
        where T : class, new()
    {
        protected NewBaseWMSObjectTest()
        {
            IsAnchor = false;
            TestDouble = -10.0;
            TestDoubleAnchor = -1.0;
            TestDecimal = -10;
            TestDecimalAnchor = -1;
            TestString = AutoTestMagicWord + typeof(T).Name;
            TestStringAnchor = AutoTestAnchorMagicWord + typeof(T).Name;
            TestGuid = new Guid();
        }

        #region .  Settings  .

        /// <summary>
        /// �����, ������� ����� ������ ��� ����� ��������� 
        /// <remarks>�������������� ������ �� �����</remarks>
        /// </summary>
        [TestFixtureTearDown]
        public void TearDown()
        {
            ClearForSelf();
        }

        #endregion

        [Test(Description = "������ ������������ ������� ������")]
        public virtual void CheckBaseObjTest()
        {
            var mgr = CreateManager();

            // ������ �� �� �� �������� �����
            var obj = mgr.Get(TestDecimalAnchor);

            if (obj != null)
                return;

            IsAnchor = true;
            var objCheck = CreateNew();
            IsAnchor = false;

            // ��� ��� ���������
            objCheck.Should().NotBeNull("�� ������� ������� ������� ������");
        }

        [Test(Description = "Manager ������ ����� ������ CRUD")]
        public virtual void ManagerCRUDTest()
        {
            var mgr = CreateManager();

            // ������� ����� ���������
            var obj = mgr.New();
            obj.Should().NotBeNull("Manager ������ ����� ��������� ����� ��������� �������");

            // ������� ��� ������ ��� �������� ������� �
            // ��������� ������������ ����
            FillRequaredFields(obj);

            // ��������� � ��
            mgr.Insert(ref obj);

            // ����������, ��� ���� ����������
            var key = ((IKeyHandler)obj).GetKey();
            key.Should().NotBeNull("� ������������ ������� ���� ������ ���� ��������");

            // ������ �� �� �� �����
            obj = mgr.Get(key);
            obj.Should().NotBeNull("�� ������ ����� �������� ������ �� �����");

            // ���������� �����
            var insKey = ((IKeyHandler)obj).GetKey();
            insKey.Should()
                .NotBeNull("���� ����������� ������ ������ ��������������� ����� �����������")
                .And.Be(key, "����� ������ ���������");

            // ���������
            MakeSimpleChange(obj);
            mgr.Update(obj);

            // ������ �� �� �� �����
            var updated = mgr.Get(key);
            updated.Should().NotBeNull("�� ������ ����� �������� ������ �� �����");

            // ���������, ��� ��������� ������
            CheckSimpleChange(obj, updated);

            // ������� �� ��
            ClearForSelf();

            // ����������, ��� ��������� �������
            var deleted = mgr.Get(key);
            deleted.Should().BeNull("��������� ������ ������ �������� �� ��");
        }

        [Test(Description = "Manager ������ ����� �������� ��� �������")]
        public virtual void ManagerGetAllTest()
        {
            var mgr = CreateManager();

            // ���������� ��� ������
            var items = mgr.GetAll();
            items.Should().NotBeNull();
            items.Should().NotBeEmpty("������ �������� ���� �� ���� �������");
        }

        [Test(Description = "Manager ������ ����� �������� ������� �� ��������")]
        public virtual void ManagerGetFilteredTest()
        {
            var mgr = CreateManager();

            // ������ �������������� ������
            var items = mgr.GetFiltered("1=2");
            items.Should().BeEmpty("��������� ������ �� ������ ��������");

            // ����� ���������� ������
            var filter = GetSimpleFilter();
            items = mgr.GetFiltered(filter);
            items.Should().NotBeEmpty("�� ������� ������ �������� ���� �� ���� ������. ��������� ������������ �������");
        }

        [Test]
        public virtual void ManagerGetHistoryTest()
        {
            var mgr = CreateManager();
            var hm = mgr as IHistoryManager<T>;
            if (hm == null)
                return;

            // �������, ����� �����������
            hm.GetHistory("1=2");
        }

        [Test(Description = "��� ���������� ����� ������ ��������� � ����� � ����")]
        public virtual void PKTypeTest()
        {
            var mgr = CreateManager();
            var item = mgr.New();
            var kh = item as IKeyHandler;
            if (kh == null)
                throw new Exception("��� ������� ������ ������������ ������ � ��������� ������");

            var managerGenericType = GetWMSBusinessObjectManagerType(mgr.GetType());
            if (managerGenericType == null)
                throw new Exception("�� ������� ���������� � ������� ������������ ��� WMSBusinessObjectManager");
            var managerKeyType = managerGenericType.GetGenericArguments()[1];

            var propertyName = kh.GetPrimaryKeyPropertyName();
            if (string.IsNullOrEmpty(propertyName))
                throw new Exception("�� ������� ���������� ��������, ������� �������� ��������� ������");

            var properties = TypeDescriptor.GetProperties(item);
            var property = properties[propertyName];
            var proprtyType = property.PropertyType.GetNonNullableType();

            proprtyType.ShouldBeEquivalentTo(managerKeyType);
        }

        [Test(Description = "������ ������ �������� �� ���� ��������� ��������")]
        public virtual void FiltersTest()
        {
            // ��������� ��������
            var properties = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>();

            // ��������� ������ �� ���� ���������
            var defFilter = string.Empty;
            var defInternalFilter = string.Empty;
            foreach (var p in properties)
            {
                var isList = typeof(IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof(string);
                var isObject = typeof(WMSBusinessObject).IsAssignableFrom(p.PropertyType);
                var dbField = SourceNameHelper.Instance.GetPropertySourceNameWithNoVirtualFields(typeof(T), p.Name);

                // ���� ������� �������� = ������ ��������� ���������
                if (isList || isObject)
                {
                    // TODO: ������ �� ��������� ��������� ��������� �������� ��������! ��� ����
                    //                    var itemType = p.PropertyType;
                    //                    if (isList)
                    //                    {
                    //                        var itemCollectionType = p.PropertyType.GetGeneticTypeFormInheritanceNode(typeof(BusinessObjectCollection<>));
                    //                        if (itemCollectionType == null)
                    //                            continue;
                    //                        itemType = itemCollectionType.GetGenericArguments()[0];
                    //                    }
                    //
                    //                    // ��������� ��� � ������ ��������� ��������� ��������
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
            // TODO: ������ �� ��������� ��������� ��������� �������� ��������! ��� ����
            //defFilter += defInternalFilter;

            // �������� ��������� ������
            var objMgr = CreateManager();
            objMgr.GetFiltered(defFilter);
            ClearForSelf();
        }

        [Test(Description = "������ ������������� CRUD'a ��������")]
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
                ex.InnerException.Message.Should().Contain("������ ������� �������������");
            }
            finally
            {
                ClearForSelf();
            }
        }

        public virtual void DeleteByParent<TParent>(object childKey, object parentKey)
        {
            if (childKey == null)
                throw new ArgumentNullException("childKey");
            if (parentKey == null)
                throw new ArgumentNullException("parentKey");

            // ������� manager'��
            var childMgr = CreateManager();
            var parentMgr = CreateManager<TParent>();

            try
            {
                // ������ ��������, ���������� �����
                CreateNew();
                var parent = parentMgr.Get(parentKey);

                // ������� ��������
                parentMgr.Delete(parent);

                // ����������, ��� ��������� �������
                var child = childMgr.Get(childKey);
                child.Should().BeNull("��� �������� {0} ������ ���� ������� ��� ��� ��������� ��������", parent);
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
            base.ClearForSelf();

            // �������� ��� ���������� �������
            var filter = GetCheckFilter();
            if (filter == null)
                return;

            // �������
            var mgr = CreateManager();
            var obj = mgr.GetFiltered(filter);
            if (obj == null) return;
            foreach (var o in obj)
                mgr.Delete(o);
        }

        public virtual T CreateNew(Action<T> customFill = null)
        {
            ClearForSelf();

            // ������� ����� ���������
            var mgr = CreateManager();
            var obj = mgr.New();
            FillRequaredFields(obj);
            if (customFill != null)
                customFill(obj);
            mgr.Insert(ref obj);
            return obj;
        }

        protected virtual string GetCheckFilter()
        {
            // ������ ����� ������
            return null;
        }

        protected virtual string GetSimpleFilter()
        {
            // �������� ��� ������
            return "1=1 and ROWNUM < 2";
        }

        protected virtual void FillRequaredFields(T obj) { }
        public virtual void PublicFillRequaredFields(T obj)
        {
            FillRequaredFields(obj);
        }

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