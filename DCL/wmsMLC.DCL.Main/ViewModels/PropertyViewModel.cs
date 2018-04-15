using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class PropertyViewModel : PanelViewModelBase, IPropertyViewModel
    {
        #region .  Fields & consts  .

        private ObservableCollection<SysObjectConfig> _selectedItems;
        private List<SysObjectConfig> _source;
        private SysObjectConfig _sumConfig;
        private readonly SysObjectWrapper _wrapper;

        #endregion .  Fields & consts  .

        #region .  Ctors  .
        public PropertyViewModel()
        {
            SaveCommand = new DelegateCustomCommand(SaveProperty, CanSaveProperty);

            Source = new List<SysObjectConfig>();
            SelectedItems = new ObservableCollection<SysObjectConfig>();

            _wrapper = new SysObjectWrapper();

            IsCustomizeBarEnabled = true;
            CreateMenu();
        }

        #endregion .  Ctors  .

        #region .  Properties  .

        public ICustomCommand SaveCommand { get; private set; }
        public ICommand AppearanceStyleCommand { get; private set; }

        public List<SysObjectConfig> Source
        {
            get { return _source; }
            set
            {
                if (_source == value)
                    return;
                _source = value;
                OnPropertyChanged("Source");
            }
        }

        public ObservableCollection<SysObjectConfig> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                if (_selectedItems == value)
                    return;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged -= SelectedItemsCollectionChanged;

                _selectedItems = value;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged += SelectedItemsCollectionChanged;

                OnPropertyChanged("SelectedItems");
            }
        }

        public SysObjectConfig SumConfig
        {
            get { return _sumConfig; }
            set
            {
                if (_sumConfig == value)
                    return;
                
                _sumConfig = value;
                OnPropertyChanged("SumConfig");
            }
        }

        public bool ShowNodeImage { get; set; }

        #endregion  .  Properties  .

        # region . Methods . 

        private void CreateMenu()
        {
            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1);
            bar.MenuItems.Add(new CommandMenuItem
            {
                IsEnable = true,
                Caption = StringResources.Save,
                Command = SaveCommand,
                ImageSmall = ImageResources.DCLSave16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLSave32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 1,
            });
        }

        public async void GetData()
        {
            try
            {
                WaitStart();
                SelectedItems = new ObservableCollection<SysObjectConfig>();
                Source = await GetSource();
            }
            finally 
            {
                WaitStop();
            }
        }

        private async Task<List<SysObjectConfig>> GetSource()
        {
            return await Task.Factory.StartNew(() =>
            {
                var result = _wrapper.GetAll();
                return result;
            });
        }

        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedItems == null || SelectedItems.Count == 0)
            {
                SumConfig = null;
                return;
            }

            SumConfig = SelectedItems[0].Clone();

            if (SelectedItems.Count == 1)
                return;

            var propertyInfo = SumConfig.GetType().GetProperties();

            for (var i = 1; i < SelectedItems.Count; i++)
            {
                var item = SelectedItems[i];

                foreach (var info in propertyInfo)
                {
                    var val1 = info.GetValue(SumConfig, null);
                    if (val1 == null)
                        continue;

                    var val2 = info.GetValue(item, null);

                    if (!val1.Equals(val2))
                        info.SetValue(SumConfig, null, null);
                }
            }
        }

        public bool CanSaveProperty()
        {
            return Source.Any(p => p.IsDirty);
        }

        public void SaveProperty()
        {
            if (!CanSaveProperty())
                return;

            try
            {
                WaitStart();
                var newElements = Source.FindAll(p => p.IsDirty);

                var newObjects = new List<SysObject>();

                foreach (var el in newElements)
                {
                    var obj = _wrapper.Get(el.ObjectID);

                    el.SaveSysObject(ref obj);
                    if (obj != null)
                    {
                        newObjects.Add(obj);
                    }
                }
                _wrapper.Update(newObjects);
                Source.ForEach(p => p.IsDirty = false);
                RaiseCommandsCanExecuteChanged();
            }
            finally
            {
                WaitStop();
            }
        }
        
        protected override void InitializeSettings()
        {
            //Используем глобальные настройки вида панели инструментов
            //MenuSuffix = GetType().GetFullNameWithoutVersion();

            base.InitializeSettings();
            IsMenuEnable = true;
        }

        protected override void SetupCustomizeMenu(BarItem bar, ListMenuItem listmenu)
        {
            base.SetupCustomizeMenu(bar, listmenu);

            if (AppearanceStyleCommand == null)
                AppearanceStyleCommand = new DelegateCustomCommand(OnAppearanceStyle, CanAppearanceStyle);

            // Добавляем функционал подсветки (в начало списка)
            var minPriority = listmenu.MenuItems.Min(i => i.Priority);
            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.AppearanceStyle,
                Command = AppearanceStyleCommand,
                ImageSmall = ImageResources.DCLAppearanceStyle16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAppearanceStyle32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = minPriority - 100
            });

            // убираем функционал настройки самого layout-а
            var item = listmenu.MenuItems.FirstOrDefault(i => i.Caption == StringResources.CustomizeRegion);
            if (item != null)
                listmenu.MenuItems.Remove(item);
        }

        private bool CanAppearanceStyle()
        {
            return !WaitIndicatorVisible && IsCustomizeEnabled;
        }

        private void OnAppearanceStyle()
        {
            if (!CanAppearanceStyle())
                return;
            IsCustomization = false;
            IsCustomization = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (Source != null)
                Source.Clear();
            if (SelectedItems != null)
                SelectedItems.Clear();
            base.Dispose(disposing);
        }

        # endregion

        # region . IPropertyViewModel .

        public void RaiseCommandsCanExecuteChanged()
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        # endregion

    }

    public interface IPropertyViewModel
    {
        void RaiseCommandsCanExecuteChanged();
    }

    public class SysObjectWrapper
    {
        private InnerClass _wrp;

        public SysObjectWrapper()
        {
            _wrp = new InnerClass();
        }

        private class InnerClass
        {
            public List<SysObjectConfig> GetAllSysObject()
            {

                IEnumerable<SysObject> objects;
                using (var objectManager = IoC.Instance.Resolve<ISysObjectManager>())
                {
                    //objects = objectManager.GetFiltered("OBJECTDATATYPE is not null AND OBJECTID > 25");
                    objects = objectManager.GetAll();
                }

                var arrayObj = (from o in objects where o != null && o.ObjectDataType != null && o.ObjectID > 25 select new SysObjectConfig(o)).ToList();
                arrayObj.ForEach(p => p.Inizialization());
                return arrayObj;
            }

            public void UpdateSysObject(IEnumerable<SysObject> objects)
            {
                using (var objectManager = IoC.Instance.Resolve<ISysObjectManager>())
                {
                    objectManager.Update(objects);
                }
            }

            public SysObject GetSysObject(object key)
            {
                using (var objectManager = IoC.Instance.Resolve<ISysObjectManager>())
                {
                    //objects = objectManager.GetFiltered("OBJECTDATATYPE is not null AND OBJECTID > 25");
                    return objectManager.Get(key);
                }
            }
        }

        public List<SysObjectConfig> GetAll()
        {
            return _wrp.GetAllSysObject();
        }

        public void Update(SysObject obj)
        {
            _wrp.UpdateSysObject(new []{obj});
        }

        public void Update(IEnumerable<SysObject> objects)
        {
            _wrp.UpdateSysObject(objects);
        }

        public SysObject Get(object key)
        {
            return  _wrp.GetSysObject(key);
        }
    }

    public class SysObjectConfig : BusinessObject, ICloneable
    {
        #region . Properties .

        private readonly List<SysObjectExt> _properties;

        #region . Object .

        [DefaultValue(false)]
        [ReadOnly(true)]
        [Category("Служебное поле")]
        public bool IsDirty { get; set; }
        
        [ReadOnly(true)]
        [Category("Объект")]
        [DisplayName(@"ID объекта")]
        public decimal ObjectID { get; set; }

        [ReadOnly(true)]
        [Category("Объект")]
        [DisplayName(@"ID родительского объекта")]
        public decimal? ObjectParentID { get; set; }

        [DisplayName(@"Имя")]
        [Category("Объект")]
        [ReadOnly(true)]
        public string ObjectName { get; set; }

        [DisplayName(@"Имя в базе данных")]
        [Category("Объект")]
        [ReadOnly(true)]
        public string ObjectDBName { get; set; }

        [DisplayName(@"Код сущности")]
        [Category("Объект")]
        [ReadOnly(true)]
        public string ObjectEntityCode { get; set; }

        [DisplayName(@"Является ключом")]
        [Category("Объект")]
        [ReadOnly(true)]
        public bool ObjectTpk { get; set; }

        [DisplayName(@"Тип")]
        [Category("Объект")]
        [ReadOnly(true)]
        public decimal? ObjectDataType { get; set; }

        [DisplayName(@"Длина")]
        [Category("Объект")]
        [ReadOnly(true)]
        public decimal? ObjectFieldLenght { get; set; }

        [DisplayName(@"Множественность")]
        [Category("Объект")]
        [ReadOnly(true)]
        public Relationship ObjectRelationShip { get; set; }

        [Category("Объект")]
        [DisplayName(@"Является вложенной коллекцией")]
        [ReadOnly(true)]
        public bool IsList { get; set; }

        [Category("Объект")]
        [DisplayName(@"Значение по умолчанию")]
        public string ObjectDefaultValue { get; set; }

        [Category("Объект")]
        [DisplayName(@"Ключевая ссылка")]
        [ReadOnly(true)]
        public string ObjectFieldKeyLink { get; set; }

        # endregion

        #region . LookUp .

        [DisplayName(@"Лукап")]
        [Category("Объект - лукап")]
        [ReadOnly(true)]
        public string ObjectLookupCode { get; set; }

        [DisplayName(@"Объект- лукап")]
        [Category("Объект - лукап")]
        [ReadOnly(true)]
        public string ObjectLookupKey { get; set; }

        [DisplayName(@"Объект- лукап")]
        [Category("Объект - лукап")]
        [ReadOnly(true)]
        public string ObjectLookupDisplay { get; set; }
        

        # endregion

        #region . ObjectExt .

        [DisplayName(@"Наименование в объекте")]
        [Category("Отображение - Вид")]
        public string ExtCaption { get; set; }
        
        [DisplayName(@"Описание в объекте")]
        [Category("Отображение - Вид")]
        public string ExtDescription { get; set; }

        [DisplayName(@"Расширенное описание в объекте")]
        [Category("Отображение - Вид")]
        public string ExtDescriptionExt { get; set; }

        [DisplayName(@"Заголовок в таблице")]
        [Category("Отображение - Вид")]
        public string ExtListName { get; set; }

        [DefaultValue(true)]
        [DisplayName(@"Отображать ли в списках")]
        [Category("Отображение")]
        public bool? View2Grid { get; set; }
     
        [DisplayName(@"Формат отображения в списках")]
        [Category("Отображение")]
        public string View2GridFormat { get; set; }

        [DefaultValue(true)]
        [DisplayName(@"Отображать ли в форме редактирования объекта")]
        [Category("Отображение")]
        public bool? View2Detail { get; set; }

        [DisplayName(@"Формат отображения в форме редактирования")]
        [Category("Отображение")]
        public string View2DetailFormat { get; set; }

        [DefaultValue(true)]
        [DisplayName(@"Отображать ли в фильтре")]
        [Category("Отображение")]
        public bool? View2Filter { get; set; }

        [DisplayName(@"Формат отображения в фильтре")]
        [Category("Отображение")]
        public string View2FilterFormat { get; set; }

        [DefaultValue(false)]
        [DisplayName(@"Отображать ли в списках через спец. поле")]
        [Category("Отображение")]
        [InfertAttribute(true)]
        public bool? View2GridAsMemo { get; set; }

        [DisplayName(@"Формат отображения в спец. поле")]
        [Category("Отображение")]
        public string View2GridAsMemoFormat { get; set; }

        [DefaultValue(false)]
        [DisplayName(@"Отображать ли в форме редактирования через спец. поле")]
        [Category("Отображение")]
        [InfertAttribute(true)]
        public bool? View2DetailAsMemo { get; set; }

        [DisplayName(@"Формат отображения в спец. поле")]
        [Category("Отображение")]
        public string View2DetailAsMemoFormat { get; set; }

        [DefaultValue(true)]
        [DisplayName(@"Отображать ли в lookup")]
        [Category("Отображение")]
        public bool? View2Lookup { get; set; }

        [DefaultValue(true)]
        [DisplayName(@"Формат отображения в lookup")]
        [Category("Отображение")]
        public string View2LookupFormat { get; set; }

        //[DisplayName("Отображать ли в списках м-к-м")]
        //[Category("2. Отображение")]
        //public bool View2Group { get; set; }

        //[DisplayName("Формат отображения в списках м-к-м")]
        //[Category("2. Отображение")]
        //public string View2GroupFormat { get; set; }

        [DisplayName(@"Показывать при редактировании")]
        [Category("Отображение")]
        public bool? ViewEnableEdit { get; set; }

        [DisplayName(@"Показывать при создании")]
        [Category("Отображение")]
        public bool? ViewEnableCreate { get; set; }

        //[ReadOnly(true)]
        //[DisplayName("Виртуальный параметр1")]
        //[Category("Виртуальность")]
        //public string VirtualFieldParamValue { get; set; }

        [DefaultValue(false)]
        [ReadOnly(true)]
        [DisplayName(@"Виртуальное поле")]
        [Category("Виртуальность")]
        public bool? IsVirtual { get; set; }

        [ReadOnly(true)]
        [DisplayName(@"Виртуальный параметр")]
        [Category("Виртуальность")]
        public string VirtualFieldName { get; set; }

        [ReadOnly(true)]
        [DisplayName(@"Связанное реальное поле")]
        [Category("Виртуальность")]
        public string RealFieldName { get; set; }

        [ReadOnly(true)]
        [Category("Валидация")]
        [DisplayName(@"Список объектов валидации")]
        public CollectionValid ObjectValidValue { get; set; }

        #endregion

        #endregion . Properties .

        # region . Ctor .

        public void Inizialization()
        {
            // Заполняем поля для лукапов
            if (!string.IsNullOrEmpty(ObjectLookupCode))
            {
                var lookupInfo = LookupHelper.GetLookupInfo(ObjectLookupCode);
                if (lookupInfo != null)
                {
                    ObjectLookupKey = lookupInfo.ValueMember;
                    ObjectLookupDisplay = lookupInfo.DisplayMember;
                }
            }

            // Определяем, является ли вложенной коллекцией
            if (ObjectDataType != null && ObjectDataType >= 1000)
            {
                using (var objectManager = IoC.Instance.Resolve<ISysObjectManager>())
                {
                    var obj = objectManager.Get(ObjectDataType);
                    if (obj == null)
                        return;
                    if (ObjectRelationShip.Equals(Relationship.Many) || obj.ObjectID >= 1000)
                        IsList = true;
                }
            }

            if (_properties == null || _properties.Count == 0)
                return;

            foreach (var ext in _properties)
            {
                switch (ext.AttrName.ToUpper())
                {
                    case "OBJECTEXTCAPTION":
                        ExtCaption = ext.AttrValue;
                        break;
                    case "OBJECTEXTDESC":
                        ExtDescription = ext.AttrValue;
                        break;
                    case "OBJECTEXTDESCEXT":
                        ExtDescriptionExt = ext.AttrValue;
                        break;
                    case "VIEW2GRID":
                        View2Grid =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "VIEW2GRIDFORMAT":
                        View2GridFormat = ext.AttrValue;
                        break;
                    case "VIEW2DETAIL":
                        View2Detail =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "VIEW2DETAILFORMAT":
                        View2DetailFormat = ext.AttrValue;
                        break;
                    case "VIEW2FILTER":
                        View2Filter =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "VIEW2FILTERFORMAT":
                        View2FilterFormat = ext.AttrValue;
                        break;
                    case "VIEW2GRIDASMEMO":
                        View2GridAsMemo =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "VIEW2GRIDASMEMOFORMAT":
                        View2GridAsMemoFormat = ext.AttrValue;
                        break;
                    case "VIEW2DETAILASMEMO":
                        View2DetailAsMemo =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "VIEW2DETAILASMEMOFORMAT":
                        View2DetailAsMemoFormat = ext.AttrValue;
                        break;
                    case "VIEW2LOOKUP":
                        View2Lookup =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "VIEW2LOOKUPFORMAT":
                        View2LookupFormat = ext.AttrValue;
                        break;
                        //case "VIEW2GROUP":
                        //    View2Group = Convert.ToBoolean(Convert.ToInt16(ext.AttrValue));
                        //    break;
                        //case "VIEW2GROUPFORMAT":
                        //    View2GroupFormat = ext.AttrValue;
                        //    break;
                    case "VIEWENABLEEDIT":
                        ViewEnableEdit =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "VIEWENABLECREATE":
                        ViewEnableCreate =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "OBJECTEXTLISTNAME":
                        ExtListName = ext.AttrValue;
                        break;
                        //case "VIRTUALFIELDPARAMVALUE":
                        //    VirtualFieldParamValue = ext.AttrValue;
                        //    break;
                    case "VIRTUALFIELDNAME":
                        VirtualFieldName = ext.AttrValue;
                        break;
                    case "REALFIELDNAME":
                        RealFieldName = ext.AttrValue;
                        break;
                    case "ISVIRTUAL":
                        IsVirtual =
                            ConvertValueBool(
                                (bool?) SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Boolean)),
                                ext.AttrName);
                        break;
                    case "OBJECTVALIDVALUE":
                        if (ext.AttrValue == null)
                            break;
                        using (var mgr = IoC.Instance.Resolve<IBaseManager<ObjectValid>>())
                        {
                            var id = SerializationHelper.ConvertToTrueType(ext.AttrValue, typeof (Decimal));
                            if (id == null)
                                return;
                            var objV = mgr.Get(id);
                            if (objV != null)
                                ObjectValidValue.Add(new ValidConfig(objV));
                        }
                        break;
                }
            }
        }
        
        public SysObjectConfig(SysObject entity)
        {
            IsDirty = false;
            IsList = false;
            View2Detail = true;
            View2Filter = true;
            View2Grid = true;
            View2GridAsMemo = false;
            View2DetailAsMemo = false;
            View2Lookup = true;
            ViewEnableCreate = true;
            ViewEnableEdit = true;

            ObjectValidValue = new CollectionValid();

            ObjectID = entity.ObjectID;
            ObjectParentID = entity.ObjectParentID;
            ObjectName = entity.ObjectName;
            ObjectDBName = entity.ObjectDBName;
            ObjectEntityCode = entity.ObjectEntityCode;
            ObjectTpk = entity.ObjectPK;
            ObjectDataType = entity.ObjectDataType;
            ObjectFieldLenght = entity.ObjectFieldLength;
            ObjectRelationShip = entity.ObjectRelationship;
            ObjectDefaultValue = entity.ObjectDefaultValue;
            ObjectFieldKeyLink = entity.ObjectFieldKeyLink;

            ObjectLookupCode = entity.ObjectLookupCode_r;

            var ext = entity.ObjectExt;
            if (ext != null)
                _properties = entity.ObjectExt.ToList();
        }

        # endregion . Ctor .

        # region . Methods .

        public void SaveSysObject (ref SysObject obj) 
        {
            if (!IsDirty)
                return;

             obj.ObjectDefaultValue = ObjectDefaultValue;
             obj.ObjectLookupCode_r = ObjectLookupCode;

            var properties = obj.ObjectExt;

            if (!string.IsNullOrEmpty(ExtCaption))
            {
                properties.Remove(properties.FirstOrDefault(p => "OBJECTEXTCAPTION".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "OBJECTEXTCAPTION", AttrValue = ExtCaption });
            }

            if (!string.IsNullOrEmpty(ExtDescription))
            {
                properties.Remove(properties.FirstOrDefault(p => "OBJECTEXTDESC".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "OBJECTEXTDESC", AttrValue = ExtDescription });
            }

            if (!string.IsNullOrEmpty(ExtDescriptionExt))
            {
                properties.Remove(properties.FirstOrDefault(p => "OBJECTEXTDESCEXT".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "OBJECTEXTDESCEXT", AttrValue = ExtDescriptionExt });
            }

            //if (View2Grid != null && View2Grid == false)
            if (View2Grid != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2GRID".Equals(p.AttrName)));
                //properties.Add(new SysObjectExt() { ObjectName = ObjectName, AttrName = "VIEW2GRID", AttrValue = (string)SerializationHelper.ConvertToTrueType(ConvertValueBool(View2Grid, "VIEW2GRID"), typeof(Byte)) });
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2GRID", AttrValue = ConvertValueBool(View2Grid, "VIEW2GRID").ToString()  });
                //properties.Add(new SysObjectExt() { ObjectName = ObjectName, AttrName = "VIEW2GRID", AttrValue = ExtDescriptionExt });
            }

            if (!string.IsNullOrEmpty(View2GridFormat))
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2GRIDFORMAT".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2GRIDFORMAT", AttrValue = View2GridFormat });
            }

            if (View2Detail != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2DETAIL".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2DETAIL", AttrValue = ConvertValueBool(View2Detail, "VIEW2DETAIL").ToString() });
            }

            if (!string.IsNullOrEmpty(View2DetailFormat))
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2DETAILFORMAT".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2DETAILFORMAT", AttrValue = View2DetailFormat });
            }

            if (View2Filter != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2FILTER".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2FILTER", AttrValue = ConvertValueBool(View2Filter, "VIEW2FILTER").ToString() });
            }

            if (!string.IsNullOrEmpty(View2FilterFormat))
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2FILTERFORMAT".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2FILTERFORMAT", AttrValue = View2FilterFormat });
            }

            if (View2GridAsMemo != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2GRIDASMEMO".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2GRIDASMEMO", AttrValue = ConvertValueBool(View2GridAsMemo, "VIEW2GRIDASMEMO").ToString() });
            }

            if (!string.IsNullOrEmpty(View2GridAsMemoFormat))
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2GRIDASMEMOFORMAT".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2GRIDASMEMOFORMAT", AttrValue = View2GridAsMemoFormat });
            }

            if (View2DetailAsMemo != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2DETAILASMEMO".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2DETAILASMEMO", AttrValue = ConvertValueBool(View2DetailAsMemo, "VIEW2DETAILASMEMO").ToString() });
            }

            if (!string.IsNullOrEmpty(View2GridAsMemoFormat))
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2DETAILASMEMOFORMAT".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2DETAILASMEMOFORMAT", AttrValue = View2GridAsMemoFormat });
            }

            if (View2Lookup != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2LOOKUP".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2LOOKUP", AttrValue = ConvertValueBool(View2Lookup, "VIEW2LOOKUP").ToString() });
            }

            if (!string.IsNullOrEmpty(View2LookupFormat))
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEW2LOOKUPFORMAT".Equals(p.AttrName)));
                properties.Add(item: new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEW2LOOKUPFORMAT", AttrValue = View2LookupFormat });
            }

            if (ViewEnableEdit != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEWENABLEEDIT".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEWENABLEEDIT", AttrValue = ConvertValueBool(ViewEnableEdit, "VIEWENABLEEDIT").ToString() });
            }

            if (ViewEnableCreate != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIEWENABLECREATE".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIEWENABLECREATE", AttrValue = ConvertValueBool(ViewEnableCreate, "VIEWENABLECREATE").ToString() });
            }

            //if (VirtualFieldParamValue != null)
            //{
            //    properties.Remove(properties.FirstOrDefault(p => "VIRTUALFIELDPARAMVALUE".Equals(p.AttrName)));
            //    properties.Add(new SysObjectExt() { ObjectName = ObjectName, AttrName = "VIRTUALFIELDPARAMVALUE", AttrValue = VirtualFieldParamValue });
            //}

            if (VirtualFieldName != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "VIRTUALFIELDNAME".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "VIRTUALFIELDNAME", AttrValue = VirtualFieldName });
            }
            if (RealFieldName != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "REALFIELDNAME".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "REALFIELDNAME", AttrValue = RealFieldName });
            }
            if (IsVirtual != null)
            {
                properties.Remove(properties.FirstOrDefault(p => "ISVIRTUAL".Equals(p.AttrName)));
                properties.Add(new SysObjectExt { ObjectName = ObjectName, AttrName = "ISVIRTUAL", AttrValue = ConvertValueBool(IsVirtual, "ISVIRTUAL").ToString() });
            }
            obj.ObjectExt = properties;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public SysObjectConfig Clone()
        {
            return (SysObjectConfig)MemberwiseClone();
        }

        private bool? ConvertValueBool(bool? value, string propName)
        {
            if (value == null)
                return null;

            var properties = TypeDescriptor.GetProperties(this);
            var atr = properties.Find(propName, true);
            if (atr == null)
                return value;

            var attributes = atr.Attributes;
            var invertAttribute = (InfertAttribute)attributes[typeof(InfertAttribute)];
            if (invertAttribute != null && invertAttribute.IsInvert)
                return !value;
            
            return value;
        }

        # endregion
    }

    public class InfertAttribute : Attribute
    {
        public bool IsInvert { get; private set; }

        public InfertAttribute(bool isInvert)
        {
            IsInvert = isInvert;
        }
    }
    
    public class CollectionValid : List<ValidConfig>
    {
    }

    public class ValidConfig
    {
        # region . Properties .

        [DisplayName(@"Ключ")]
        [ReadOnly(true)]
        public decimal ObjectValidId { get; set; }

        [DisplayName(@"Наименование")]
        [ReadOnly(true)]
        public string ObjectValidName { get; set; }

        [DisplayName(@"Уровень валидации")]
        [ReadOnly(true)]
        public ValidateErrorLevel ObjectValidLevel { get; set; }

        [DisplayName(@"Сообщение для пользователя")]
        [ReadOnly(true)]
        public string ObjectValidMessage { get; set; }

        [DisplayName(@"Параметр")]
        [ReadOnly(true)]
        public string ObjectValidParameters { get; set; }
        
        [DisplayName(@"Значение")]
        [ReadOnly(true)]
        public string ObjectValidValue { get; set; }

        [DisplayName(@"Приоритет")]
        [ReadOnly(true)]
        public decimal ObjectValidPriority { get; set; }

        # endregion

        # region . Ctor .

        public ValidConfig(ObjectValid entity)
        {
            ObjectValidId = entity.ObjectValidId;
            ObjectValidName = entity.ObjectValidName;
            ObjectValidLevel = entity.ObjectValidLevel;
            ObjectValidMessage = entity.ObjectValidMessage;
            ObjectValidParameters = entity.ObjectValidParameters;
            ObjectValidValue = entity.ObjectValidValue;
            ObjectValidPriority = entity.ObjectValidPriority;
        }

        # endregion
    }

}