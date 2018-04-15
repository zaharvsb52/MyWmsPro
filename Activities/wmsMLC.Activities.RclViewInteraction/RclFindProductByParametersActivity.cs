using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using log4net;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclFindProductByParametersActivity : NativeActivity
    {
        #region .  Fields  .
        private const string FilterModelPropertyName = "Filter";
        private const string ProductListModelPropertyName = "ProductList";
        private const string FilterValueListModelPropertyName = "FilterValueList";
        private const string ItemsSourceModelPropertyName = "ItemsSource";
        private const string MenuAutoFilter = "MenuAutoFilter";
        private const byte DefaultGridMaxRow = 50;
        private const string DefaultGridProductLayout = @"<?xml version=""1.0"" encoding=""utf-16""?><CustomDataLayoutControl ID=""objectDataLayout"" Orientation=""Vertical""><Element ID=""ProductList"" VerticalAlignment=""Stretch"" Label="""" ProductList_RclGridControl_RclGridControl=""&lt;XtraSerializer version=&quot;1.0&quot; application=&quot;GridControl&quot;&gt;&#xD;&#xA;  &lt;property name=&quot;#LayoutVersion&quot;&gt;1.0.0.0&lt;/property&gt;&#xD;&#xA;  &lt;property name=&quot;$GridControl&quot; iskey=&quot;true&quot; value=&quot;GridControl&quot;&gt;&#xD;&#xA;    &lt;property name=&quot;Columns&quot; iskey=&quot;true&quot; value=&quot;6&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;Item1&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IsSelected&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;22&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item2&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;1&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;SKUID_R_NAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item3&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;2&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;PRODUCTCOUNTSKU&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item4&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;PRODUCTCOUNT&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item5&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VARTDESC&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;195&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item6&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VQLFNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;SortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;GroupCount&quot;&gt;0&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;View&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;ShowGroupPanel&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowColumnFiltering&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AutoWidth&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;SearchPanelHighlightResults&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;IsRowCellMenuEnabled&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowSearchPanelCloseButton&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ItemsSourceErrorInfoShowMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowEditing&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowFilterPanelMode&quot;&gt;Never&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;NavigationStyle&quot;&gt;Cell&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ColumnChooserState&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;Size&quot;&gt;220,250&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Location&quot;&gt;265,70&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowCascadeUpdate&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowPerPixelScrolling&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowIndicator&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;FormatConditions&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;      &lt;property name=&quot;ScrollAnimationMode&quot;&gt;Linear&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowAutoFilterRow&quot;&gt;true&lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;SelectionMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;TotalSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;MRUFilters&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummarySortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;  &lt;/property&gt;&#xD;&#xA;&lt;/XtraSerializer&gt;"" /><AvailableItems /></CustomDataLayoutControl>";
        private const string SelectProductDialogTitle = "Выбор товара";
        private const string ArtNotFoundFormat = "Артикул с номером {0} не найден.";
        private const string IwbNotFoundFormat = "Приходная накладная с номером {0} не найдена.";
        private const string OwbNotFoundFormat = "Расходная накладная с номером {0} не найдена.";
        private const string IsSelectedPropertyName = "IsSelected";

        private NativeActivityContext _context;
        private static readonly Dictionary<Type, Parameter> FilterParameters;
        private readonly string[] _defaultGridFieldNames;
        private readonly ILog _log = LogManager.GetLogger(typeof(RclFindProductByParametersActivity));

        private List<Filter<SKUBC>> _skuBarcodes;
        private List<Filter<Art>> _arts;
        private List<Filter<TE>> _tes;
        private List<Filter<Place>> _places;
        private List<Filter<IWB>> _iwbs;
        private List<Filter<OWB>> _owbs;
        private List<Filter<CargoIWB>> _cargoIwbs;
        private List<Filter<CargoOWB>> _cargoOwbs;

        private List<Product> _result;
        private object _oldFilterValue;
        private string _gridProductLayoutInternal;
        private decimal? _mandantId;
        private string _mandantCode;
        #endregion .  Fields  .

        #region .ctors .
        static RclFindProductByParametersActivity()
        {
            Func<object, IViewService, string, string, double, bool> validateTypeDecimalHandler = (value, viewService, title, message, fontSize) =>
            {
                decimal id;
                if (!decimal.TryParse((string) value, out id))
                {
                    ShowError(viewService, title, string.Format(message, value), fontSize);
                    return false;
                }
                return true;
            };

            FilterParameters = new Dictionary<Type, Parameter>
            {
                {
                    typeof (SKUBC),
                    new Parameter(SKUBC.BarcodeValuePropertyName, typeof (string),
                        "Осуществлён пустой ввод. Введите ШК артикула.")
                },
                {
                    typeof (Art),
                    new Parameter(Art.ArtNamePropertyName, typeof (string),
                        "Осуществлён пустой ввод. Введите номер артикула.")
                },
                {
                    typeof (TE),
                    new Parameter(new TE().GetPrimaryKeyPropertyName(), typeof (string),
                        "Осуществлён пустой ввод. Введите номер ТЕ.")
                },
                {
                    typeof (Place),
                    new Parameter(new Place().GetPrimaryKeyPropertyName(), typeof (string),
                        "Осуществлён пустой ввод. Введите код места.")
                },
                {
                    typeof (IWB),
                    new Parameter(IWB.IWBNAMEPropertyName, typeof (string),
                        "Осуществлён пустой ввод. Введите номер приходной накладной.")
                },
                {
                    typeof (OWB),
                    new Parameter(OWB.OWBNAMEPropertyName, typeof (string),
                        "Осуществлён пустой ввод. Введите номер расходной накладной.")
                },
                {
                    typeof (CargoIWB),
                    new Parameter(new CargoIWB().GetPrimaryKeyPropertyName(), typeof (decimal),
                        "Осуществлён пустой ввод. Введите входящий груз.")
                    {
                        ValidateType = validateTypeDecimalHandler,
                        ValidateTypeMessageFormat = "Ошибочный код входящего груза '{0}'."
                    }
                },
                {
                    typeof (CargoOWB),
                    new Parameter(new CargoOWB().GetPrimaryKeyPropertyName(), typeof (decimal),
                        "Осуществлён пустой ввод. Введите исходящий груз.")
                    {
                        ValidateType = validateTypeDecimalHandler,
                        ValidateTypeMessageFormat = "Ошибочный код исходящего груза '{0}'."
                    }
                }
            };
        }

        public RclFindProductByParametersActivity()
        {
            DisplayName = "ТСД: Поиск товара по параметрам";
            DisplaySetting = SettingDisplay.List;
            _defaultGridFieldNames = new[] {"SKUID_R_NAME", "PRODUCTCOUNTSKU", "PRODUCTCOUNT", "VARTDESC", "VQLFNAME"};
            IsTeCodeToUpper = true;
            GridMaxRow = DefaultGridMaxRow;
            FilterValuesMaxRow = 5;
            InputFiltersIsReadOnly = false;
            MandantId = null;
        }
        #endregion .ctors .

        #region .  Properties  .
        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Список ШК артикула")]
        public InArgument<List<SKUBC>> SkuBarcodes { get; set; }

        [DisplayName(@"Список артикулов")]
        public InArgument<List<Art>> Arts { get; set; }

        [DisplayName(@"Список ТЕ")]
        public InArgument<List<TE>> Tes { get; set; }

        [DisplayName(@"Список мест")]
        public InArgument<List<Place>> Places { get; set; }

        [DisplayName(@"Список приходных накладных")]
        public InArgument<List<IWB>> Iwbs { get; set; }

        [DisplayName(@"Список расходных накладных")]
        public InArgument<List<OWB>> Owbs { get; set; }

        [DisplayName(@"Список входящих грузов")]
        public InArgument<List<CargoIWB>> CargoIwbs { get; set; }

        [DisplayName(@"Список исходящих грузов")]
        public InArgument<List<CargoOWB>> CargoOwbs { get; set; }

        [DisplayName(@"Одна строка товара на выходе")]
        public InArgument<bool> IsSelectModeSingleRow { get; set; }

        [DisplayName(@"Вид списка полей для списка товара")]
        public SettingDisplay DisplaySetting { get; set; }

        [DisplayName(@"Имена полей для списка товара")]
        [Description(@"Если список имен полей не определен, то будет использоваться список имен полей по умолчанию (""SKUID_R_NAME"", ""PRODUCTCOUNTSKU"", ""PRODUCTCOUNT"", ""VARTDESC"", ""VQLFNAME"").")]
        public InArgument<string[]> GridFieldNames { get; set; }

        [DisplayName(@"Преобразовывать номер ТЕ в верхний регистр при вводе фильтра")]
        public InArgument<bool> IsTeCodeToUpper { get; set; }

        [DisplayName(@"Максимальное количество записей в списке товара (по умолчанию - 50).")]
        [DefaultValue(50)]
        public byte GridMaxRow { get; set; }

        [DisplayName(@"Максимальное количество записей значений фильтра на странице (по умолчанию - 5).")]
        [DefaultValue(5)]
        public byte FilterValuesMaxRow { get; set; }

        [DisplayName(@"Настройка вида списка товара. Если значение нерпределено, то подставляется значение по-умолчанию.")]
        public InArgument<string> GridProductLayout { get; set; }

        [DisplayName(@"Запрещать изменение заданных фильтров.")]
        [DefaultValue(false)]
        public InArgument<bool> InputFiltersIsReadOnly { get; set; }

        [DisplayName(@"Мандант")]
        [DefaultValue(null)]
        public InArgument<decimal?> MandantId { get; set; }

        [DisplayName(@"Результат")]
        public OutArgument<List<Product>> Products { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsSelectModeSingleRow, type.ExtractPropertyName(() => IsSelectModeSingleRow));
            ActivityHelpers.AddCacheMetadata(collection, metadata, GridFieldNames, type.ExtractPropertyName(() => GridFieldNames));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsTeCodeToUpper, type.ExtractPropertyName(() => IsTeCodeToUpper));
            ActivityHelpers.AddCacheMetadata(collection, metadata, GridProductLayout, type.ExtractPropertyName(() => GridProductLayout));

            ActivityHelpers.AddCacheMetadata(collection, metadata, SkuBarcodes, type.ExtractPropertyName(() => SkuBarcodes));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Arts, type.ExtractPropertyName(() => Arts));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Tes, type.ExtractPropertyName(() => Tes));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Places, type.ExtractPropertyName(() => Places));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Iwbs, type.ExtractPropertyName(() => Iwbs));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Owbs, type.ExtractPropertyName(() => Owbs));
            ActivityHelpers.AddCacheMetadata(collection, metadata, CargoIwbs, type.ExtractPropertyName(() => CargoIwbs));
            ActivityHelpers.AddCacheMetadata(collection, metadata, CargoOwbs, type.ExtractPropertyName(() => CargoOwbs));

            ActivityHelpers.AddCacheMetadata(collection, metadata, InputFiltersIsReadOnly, type.ExtractPropertyName(() => InputFiltersIsReadOnly));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MandantId, type.ExtractPropertyName(() => MandantId));
            
            ActivityHelpers.AddCacheMetadata(collection, metadata, Products, type.ExtractPropertyName(() => Products));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            _context = context;

            _gridProductLayoutInternal = context.GetValue(GridProductLayout);
            if (string.IsNullOrEmpty(_gridProductLayoutInternal))
                _gridProductLayoutInternal = DefaultGridProductLayout;

            _mandantId = MandantId.Get(context);
            if (_mandantId.HasValue)
            {
                try
                {
                    Mandant mandant;
                    using (var mng = IoC.Instance.Resolve<IBaseManager<Mandant>>())
                    {
                        mandant = mng.Get(_mandantId.Value,
                            FilterHelper.GetAttrEntity<Mandant>(Mandant.MANDANTCODEPropertyName));
                    }

                    if (mandant != null)
                        _mandantCode = mandant.MandantCode;
                }
                catch (Exception ex)
                {
                    _log.Debug("Ошибка при получении манданта.", ex);
                }
                finally
                {
                    if (string.IsNullOrEmpty(_mandantCode))
                        _mandantCode = string.Format("Id: {0}", _mandantId);
                }
            }

            InitializeParameters();
            _result = new List<Product>();

            List<ProductPl> productList;

            //Передан хоть один внешний параметр
            if (ValidateParameters())
            {
                try
                {
                    productList = FilterApply();
                }
                catch
                {
                    productList = null;
                }

                if (ShowSelectProductDialog(productList))
                {
                    SaveParameters();
                }
            }
            else
            {
                //Предварительная фильтрация
                while (true)
                {
                    if (ShowFilterDialog())
                    {
                        //Фильтрация товара
                        try
                        {
                            productList = FilterApply();
                        }
                        catch
                        {
                            //Если произошла ошибка получения данных, то показываем ошибку и возвращаемся на предыдущую форму
                            InitializeParameters();
                            continue;
                        }

                        //Выбор товара
                        if (ShowSelectProductDialog(productList))
                        {
                            SaveParameters();
                        }
                    }
                    break;
                }
            }

            //Подготовка результата
            PrepareResult();
        }

        private void InitializeParameters()
        {
            var isReadOnly = _context.GetValue(InputFiltersIsReadOnly);
            _skuBarcodes = DistinctParameter(_context.GetValue(SkuBarcodes), isReadOnly);
            _arts = DistinctParameter(_context.GetValue(Arts), isReadOnly);
            _tes = DistinctParameter(_context.GetValue(Tes), isReadOnly);
            _places = DistinctParameter(_context.GetValue(Places), isReadOnly);
            _iwbs = DistinctParameter(_context.GetValue(Iwbs), isReadOnly);
            _owbs = DistinctParameter(_context.GetValue(Owbs), isReadOnly);
            _cargoIwbs = DistinctParameter(_context.GetValue(CargoIwbs), isReadOnly);
            _cargoOwbs = DistinctParameter(_context.GetValue(CargoOwbs), isReadOnly);
        }

        private bool ValidateParameters()
        {
            return _skuBarcodes.Any() || _arts.Any() || _tes.Any() || _places.Any() ||
                _iwbs.Any() || _owbs.Any() || _cargoIwbs.Any() || _cargoOwbs.Any();
        }

        private void SaveParameters()
        {
            SaveParameters(SkuBarcodes, _skuBarcodes);
            SaveParameters(Arts, _arts);
            SaveParameters(Tes, _tes);
            SaveParameters(Places, _places);
            SaveParameters(Iwbs, _iwbs);
            SaveParameters(Owbs, _owbs);
            SaveParameters(CargoIwbs, _cargoIwbs);
            SaveParameters(CargoOwbs, _cargoOwbs);
        }

        private void SaveParameters<T>(InArgument<List<T>> argParameters, List<Filter<T>> filters) where T : WMSBusinessObject
        {
            if (filters.Any(p => p.Value != null))
            {
                var parameters = _context.GetValue(argParameters);
                if (parameters != null)
                {
                    parameters.Clear();
                    parameters.AddRange(filters.Where(p => p.Value != null).Select(p => p.Value));
                }
            }
        }

        //Подготовка результата
        private void PrepareResult()
        {
            Products.Set(_context, _result);
        }

        //Предварительная фильтрация часть 1
        private bool ShowFilterDialog()
        {
            var footerMenu = CreateFilterDialogFooterMenuPart1();
            var model = CreateFilterDialogModel(footerMenu);
            var viewService = IoC.Instance.Resolve<IViewService>();
            var fontSize = FontSize.Get(_context);
            model[FilterModelPropertyName] = _oldFilterValue;

            while (true)
            {
                if (viewService.ShowDialogWindow(model, false) != true)
                    return false;

                var filterValue = model[FilterModelPropertyName];
                _oldFilterValue = filterValue;

                switch (model.MenuResult)
                {
                    case "F1": //ШК артикула
                        if (!GetParameter(typeof(SKUBC)).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (!ValidateValue(_skuBarcodes, filterValue, viewService, model.PanelCaption))
                            continue;

                        AddParameterValue(_skuBarcodes, filterValue);
                        return true;
                    case "F2": //Артикул
                        if (!GetParameter(typeof (Art)).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (!ValidateValue(_arts, filterValue, viewService, model.PanelCaption))
                            continue;

                        if (GetFilteredEntity("Артикул", _arts, filterValue, Art.MANDANTIDPropertyName, viewService, model.PanelCaption, ArtNotFoundFormat, fontSize))
                            return true;
                        continue;
                    case "F3": //ТЕ
                        if (!GetParameter(typeof(TE)).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        var tecode = (string) filterValue;
                        tecode = IsTeCodeToUpper.Get(_context) ? tecode.ToUpper() : tecode;
                        if (!ValidateValue(_tes, tecode, viewService, model.PanelCaption))
                            continue;

                        AddParameterValue(_tes, tecode);
                        return true;
                    case "F4": //Место
                        if (!GetParameter(typeof(Place)).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (!ValidateValue(_places, filterValue, viewService, model.PanelCaption))
                            continue;

                        AddParameterValue(_places, filterValue);
                        return true;
                    case "F5": //Ещё
                        switch (ShowFilterDialogPart2())
                        {
                            case "1":
                                return true;
                            default:
                                continue;
                        }
                        
                    default:
                        return false;
                }
            }
        }

        //Предварительная фильтрация часть 2
        private string ShowFilterDialogPart2()
        {
            var footerMenu = CreateFilterDialogFooterMenuPart2();
            var model = CreateFilterDialogModel(footerMenu);
            var viewService = IoC.Instance.Resolve<IViewService>();
            var fontSize = FontSize.Get(_context);
            model[FilterModelPropertyName] = _oldFilterValue;

            while (true)
            {
                if (viewService.ShowDialogWindow(model, false) != true)
                    return "0";

                var filterValue = model[FilterModelPropertyName];

                Type entitytype;
                switch (model.MenuResult)
                {
                    case "F1": //Приход
                        entitytype = typeof (IWB);
                        if (!GetParameter(entitytype).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (!ValidateValue(_iwbs, filterValue, viewService, model.PanelCaption))
                            continue;

                        if (GetFilteredEntity("Приход", _iwbs, filterValue, IWB.MANDANTIDPropertyName, viewService, model.PanelCaption, IwbNotFoundFormat, fontSize))
                            return "1";
                        continue;
                    case "F2": //Расход
                        entitytype = typeof(OWB);
                        if (!GetParameter(entitytype).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (!ValidateValue(_owbs, filterValue, viewService, model.PanelCaption))
                            continue;

                        if (GetFilteredEntity("Расход", _owbs, filterValue, OWB.MANDANTIDPropertyName, viewService, model.PanelCaption, OwbNotFoundFormat, fontSize))
                            return "1";
                        continue;
                    case "F3": //Вх. груз
                        if (!GetParameter(typeof(CargoIWB)).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (!ValidateValue(_cargoIwbs, filterValue, viewService, model.PanelCaption))
                            continue;

                        AddParameterValue(_cargoIwbs, filterValue);
                        return "1";
                    case "F4": //Исх. груз
                        if (!GetParameter(typeof(CargoOWB)).Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (!ValidateValue(_cargoOwbs, filterValue, viewService, model.PanelCaption))
                            continue;

                        AddParameterValue(_cargoOwbs, filterValue);
                        return "1";
                    case "F5": //Ещё
                        return "1F5";
                    default:
                        return "0";
                }
            }
        }

        //Модель диалога предварительной фильтрации
        private DialogSourceViewModel CreateFilterDialogModel(ValueDataField[] footerMenu)
        {
            var result = new DialogSourceViewModel
            {
                PanelCaption = "Предварительный фильтр",
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false
            };

            var field = new ValueDataField
            {
                Name = FilterModelPropertyName,
                Caption = "Фильтр",
                FieldType = typeof(string),
                IsEnabled = true,
                SetFocus = true
            };
            field.Set(ValueDataFieldConstants.IsDoNotMovedFocusOnNextControl, true);
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Message",
                FieldType = typeof(string),
                LabelPosition = "Top",
                IsEnabled = false,
                Value = "Введите значение и выберите способ фильтрации"
            };
            AddField(result, field);

            if (footerMenu != null)
            {
                field = new ValueDataField
                {
                    Name = ValueDataFieldConstants.FooterMenu,
                    Caption = ValueDataFieldConstants.FooterMenu,
                    FieldType = typeof (FooterMenu),
                    IsEnabled = true
                };
                field.Set(ValueDataFieldConstants.FooterMenu, footerMenu);
                AddField(result, field);
            }

            result.UpdateSource();
            return result;
        }

        //Выбор товара
        private bool ShowSelectProductDialog(ICollection<ProductPl> productList)
        {
            var isSelectModeSingleRow = IsSelectModeSingleRow.Get(_context);
            var showAutoFilterRow = false;
            var oldShowAutoFilterRow = false;
            var footerMenu = CreateSelectProductDialogFooterMenu();
            var model = CreateSelectProductDialogModel(footerMenu, productList, false);
            footerMenu[0].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() =>
            {
                var id = model[ProductListModelPropertyName] as decimal?;
                if (!id.HasValue) 
                    return;
                var row = productList.FirstOrDefault(p => p.GetKey<decimal>() == id.Value);
                if (row == null) 
                    return;
                row.IsSelected = !row.IsSelected;
                if (isSelectModeSingleRow && productList.Count(p => p.IsSelected) > 1)
                    row.IsSelected = false;
            }));

            var viewService = IoC.Instance.Resolve<IViewService>();

            while (true)
            {
                if (viewService.ShowDialogWindow(model, false) != true)
                    return false;

                switch (model.MenuResult)
                {
                    case "Value": //Двойной клик мышки
                    case "Return": //Применить
                        _result = productList.Where(p => p.IsSelected).Cast<Product>().ToList();
                        if (!_result.Any())
                        {
                            ShowError(SelectProductDialogTitle, "Не выбран товар. Выберите товар.");
                            continue;
                        }
                        return true;
                    case "F2": //Фильтр
                        if (model.ContainsKey(ValueDataFieldConstants.Properties))
                        {
                            var properties = (IDictionary<string, object>) model[ValueDataFieldConstants.Properties];
                            if (properties.ContainsKey(ValueDataFieldConstants.ShowAutoFilterRow))
                            {
                                oldShowAutoFilterRow =
                                    showAutoFilterRow = properties[ValueDataFieldConstants.ShowAutoFilterRow].To(false);
                            }
                        }

                        //Выбор фильтра.
                        var isError = false;
                        while (true)
                        {
                            bool hasFilterChanged;
                            ShowSelectFilterDialog(out hasFilterChanged, ref showAutoFilterRow);

                            if (!hasFilterChanged && oldShowAutoFilterRow == showAutoFilterRow && !isError)
                                break;

                            oldShowAutoFilterRow = showAutoFilterRow;
                            var field = model.GetField(ProductListModelPropertyName);
                            if (field == null)
                                throw new DeveloperException("Не найдено поле '{0}' в модели '{1}'.",
                                    ProductListModelPropertyName, model.PanelCaption);

                            field.Set(ValueDataFieldConstants.DoNotAllowUpdateShowAutoFilterRowFromXml, true);
                            field.Set(ValueDataFieldConstants.ShowAutoFilterRow, showAutoFilterRow);

                            if (hasFilterChanged || isError)
                            {
                                try
                                {
                                    productList = FilterApply();
                                }
                                catch
                                {
                                    //Если произошла ошибка получения данных, то показываем ошибку и возвращаемся на предыдущую форму
                                    isError = true;
                                    continue;
                                }
                                field.Set(ValueDataFieldConstants.ItemsSource, productList);
                            }

                            model.UpdateSource();
                            break;
                        }
                            
                        continue;

                    default:
                        return false;
                }
            }
        }

        //Модель диалога выбора товара
        private DialogSourceViewModel CreateSelectProductDialogModel(ValueDataField[] footerMenu, ICollection<ProductPl> productList, bool showAutoFilterRow)
        {
            var result = new DialogSourceViewModel
            {
                PanelCaption = SelectProductDialogTitle,
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
                LayoutValue = _gridProductLayoutInternal
            };

            var field = new ValueDataField
            {
                Name = ProductListModelPropertyName,
                //Caption = "Список товара",
                Caption = string.Empty,
                FieldType = typeof(Product),
                LabelPosition = "Left",
                IsEnabled = true,
                SetFocus = true,
                CloseDialog = true
            };
            field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
            field.Set(ValueDataFieldConstants.ShowControlMenu, false);
            field.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, false);
            field.Set(ValueDataFieldConstants.ShowAutoFilterRow, showAutoFilterRow);
            field.Set(ValueDataFieldConstants.DoNotActionOnEnterKey, true);
            field.Set(ValueDataFieldConstants.ItemsSource, productList);

            //Получаем поля для грида
            var gridFieldNames = GridFieldNames.Get(_context);
            var gridFields = GetGridFields(field.FieldType, DisplaySetting,
                gridFieldNames != null && gridFieldNames.Any() ? gridFieldNames : _defaultGridFieldNames);

            var selectField = new DataField
            {
                Name = IsSelectedPropertyName,
                BindingPath = IsSelectedPropertyName,
                Caption = string.Empty,
                EnableEdit = false,
                IsEnabled = true,
                FieldName = IsSelectedPropertyName,
                FieldType = typeof(bool),
                SourceName = IsSelectedPropertyName
            };
            gridFields.Insert(0, selectField);

            field.Set(ValueDataFieldConstants.Fields, gridFields.ToArray());
            if (footerMenu != null)
                field.Set(ValueDataFieldConstants.FooterMenu, footerMenu);

            AddField(result, field);

            result.UpdateSource();
            return result;
        }

        //Выбор полей списка товара
        private List<DataField> GetGridFields(Type type, SettingDisplay displaySetting, string[] propertyNames)
        {
            var fieldList = DataFieldHelper.Instance.GetDataFields(type, displaySetting);
            if (propertyNames == null)
                return new List<DataField>(fieldList);

            return propertyNames.Select(propertyName => fieldList.FirstOrDefault(p => p.Name.EqIgnoreCase(propertyName)))
                    .Where(field => field != null)
                    .ToList();
        }
        

        //Фильтрация товара
        private List<ProductPl> FilterApply()
        {
            const string and = " and ";

            try
            {
                var where = new StringBuilder();
                var whereand = false;

                if (_mandantId.HasValue)
                {
                    where.AppendFormat("MANDANTID = {0}", _mandantId);
                    whereand = true;
                }

                if (_skuBarcodes.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_skuBarcodes, true);
                    where.AppendFormat("{0}WMSPRODUCT.SKUID_R IN (SELECT DISTINCT S.SKUID FROM WMSSKU S JOIN WMSBARCODE B ON TO_CHAR(S.SKUID)=B.BARCODEKEY WHERE B.BARCODE2ENTITY='SKU' AND {1})",
                        whereand ? and : null,
                        FilterHelper.GetFilterIn("B.BARCODEVALUE", filtervalues));
                    whereand = true;
                }

                if (_arts.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_arts);
                    where.AppendFormat("{0}{1}", whereand ? and : null, FilterHelper.GetFilterIn("ARTCODE_R", filtervalues));
                    whereand = true;
                }

                if (_tes.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_tes);
                    where.AppendFormat("{0}{1}", whereand ? and : null, FilterHelper.GetFilterIn("WMSPRODUCT.TECODE_R", filtervalues));
                    whereand = true;
                }

                if (_places.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_places);
                    where.AppendFormat("{0}{1}", whereand ? and : null, FilterHelper.GetFilterIn("VPLACECODE", filtervalues));
                    whereand = true;
                }

                if (_iwbs.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_iwbs);
                    where.AppendFormat("{0}WMSPRODUCT.IWBPOSID_R  IN (SELECT DISTINCT IP.IWBPOSID FROM WMSIWBPOS IP WHERE IP.IWBPOSID = WMSPRODUCT.IWBPOSID_R AND {1})", 
                        whereand ? and : null, FilterHelper.GetFilterIn("IP.IWBID_R", filtervalues));
                    whereand = true;
                }

                if (_owbs.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_owbs);
                    where.AppendFormat("{0}{1}", whereand ? and : null, FilterHelper.GetFilterIn("VOWBID", filtervalues));
                    whereand = true;
                }

                if (_cargoIwbs.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_cargoIwbs);
                    where.AppendFormat(
                        "{0}WMSPRODUCT.IWBPOSID_R IN (SELECT DISTINCT IP.IWBPOSID FROM WMSIWBPOS IP JOIN WMSIWB2CARGO IC ON IP.IWBID_R=IC.IWBID_R WHERE IP.IWBPOSID=WMSPRODUCT.IWBPOSID_R AND {1})",
                        whereand ? and : null, FilterHelper.GetFilterIn("IC.CARGOIWBID_R", filtervalues));
                    whereand = true;
                }

                if (_cargoOwbs.Any(p => p.Value != null))
                {
                    var filtervalues = GetParameterValues(_cargoOwbs);
                    where.AppendFormat(
                        "{0}WMSPRODUCT.OWBPOSID_R IN (SELECT DISTINCT OP.OWBPOSID FROM WMSOWBPOS OP JOIN WMSOWB2CARGO OC ON OP.OWBID_R=OC.OWBID_R WHERE OP.OWBPOSID=WMSPRODUCT.OWBPOSID_R AND {1})",
                        whereand ? and : null, FilterHelper.GetFilterIn("OC.CARGOOWBID_R", filtervalues));
                    whereand = true;
                }

                var filter = string.Format("{0}{1}{2}", where, whereand ? and : null, FilterHelper.GetFetchCountFilter(GridMaxRow));

                Product[] result;
                using (var productManager = IoC.Instance.Resolve<IBaseManager<Product>>())
                {
                     result = productManager.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                }

                return result.Select(p => new ProductPl(p)).ToList();
            }
            catch(Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);

                _log.Debug(ex);
                _log.Warn(message);
                ShowError("Ошибка применения фильтра", message);
                throw new OperationException(message);
            }
        }

        //Выбор фильтра.
        private void ShowSelectFilterDialog(out bool hasFilterChanged, ref bool showAutoFilterRow)
        {
            hasFilterChanged = false;
            var model = new DialogSourceViewModel
            {
                PanelCaption = "Выбор фильтра",
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
                Fields = CreateSelectFilterDialogFields(showAutoFilterRow)
            };
            model.UpdateSource();

            var hasFilterChangedInternal = false;
            var viewService = IoC.Instance.Resolve<IViewService>();

            while (true)
            {
                if (hasFilterChangedInternal)
                {
                    model.Fields = CreateSelectFilterDialogFields(showAutoFilterRow);
                    model.UpdateSource();
                }

                if (viewService.ShowDialogWindow(model, false) != true)
                    return;

                var field = model.Fields.FirstOrDefault(p => Equals(p.Value, model.MenuResult));
                var filterName = field == null ? null : field.Caption;

                switch (model.MenuResult)
                {
                    case "D1": //ШК артикула
                        ShowUpdateFilterDialog(filterName, _skuBarcodes, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D2": //Артикул
                        ShowUpdateFilterDialog(filterName, _arts, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D3": //ТЕ
                        ShowUpdateFilterDialog(filterName, _tes, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D4": //Место
                        ShowUpdateFilterDialog(filterName, _places, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D5": //Приход
                        ShowUpdateFilterDialog(filterName, _iwbs, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D6": //Расход
                        ShowUpdateFilterDialog(filterName, _owbs, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D7": //Вх. груз
                        ShowUpdateFilterDialog(filterName, _cargoIwbs, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D8": //Исх. груз
                        ShowUpdateFilterDialog(filterName, _cargoOwbs, out hasFilterChangedInternal);
                        hasFilterChanged |= hasFilterChangedInternal;
                        continue;
                    case "D9": //Автофильтр
                        showAutoFilterRow = !showAutoFilterRow;
                        hasFilterChangedInternal = true;
                        continue;
                    default:
                        return;
                }
            }
        }

        //Создание полей модели диалога выбора фильтра
        private List<ValueDataField> CreateSelectFilterDialogFields(bool showAutoFilterRow)
        {
            var result = new List<ValueDataField>();
            var field = new ValueDataField
            {
                Name = "Menu0",
                Caption = "ШК артикула",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                SetFocus = true,
                Value = "D1"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_skuBarcodes));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu1",
                Caption = "Артикул",
                FieldType = typeof (CustomButton),
                IsEnabled = true,
                Value = "D2"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_arts));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu2",
                Caption = "ТЕ",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "D3"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_tes));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu3",
                Caption = "Место",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "D4"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_places));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu4",
                Caption = "Приход",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "D5"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_iwbs));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu5",
                Caption = "Расход",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "D6"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_owbs));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu6",
                Caption = "Вх. груз",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "D7"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_cargoIwbs));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu7",
                Caption = "Исх. груз",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "D8"
            };
            field.Set(ValueDataFieldConstants.SuffixText, GetFilterIndicator(_cargoOwbs));
            AddField(result, field);

            field = new ValueDataField
            {
                Name = MenuAutoFilter,
                Caption = "Автофильтр",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "D9"
            };
            field.Set(ValueDataFieldConstants.SuffixText, showAutoFilterRow ? "вкл." : "выкл.");
            AddField(result, field);

            field = new ValueDataField
            {
                Name = "Menu9",
                Caption = "Назад",
                FieldType = typeof(CustomButton),
                IsEnabled = true,
                Value = "Escape"
            };
            AddField(result, field);

            return result;
        }

        //Диалог управления фильтром
        private void ShowUpdateFilterDialog<T>(string filterName, List<Filter<T>> filter, out bool hasFilterChanged) where T : WMSBusinessObject
        {
            hasFilterChanged = false;
            var footerMenu = CreateUpdateFilterDialogFooterMenu();
            footerMenu[0].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() => { }));
            footerMenu[1].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() => { }));

            var model = CreateUpdateFilterDialogModel(footerMenu, string.Format("Управление фильтром '{0}'", filterName));
            var viewService = IoC.Instance.Resolve<IViewService>();
            var fontSize = FontSize.Get(_context);

            var typeEntity = typeof (T);
            var parameter = GetParameter(typeEntity);
            var propertyName = parameter.PropertyName;

            var filterListField = model.GetField(FilterValueListModelPropertyName);
            if (filterListField == null)
                throw new DeveloperException("Не найдено поле '{0}' в модели '{1}'.", FilterValueListModelPropertyName, model.PanelCaption);

            filterListField.Set(ValueDataFieldConstants.ValueMember, propertyName);
            filterListField.Set(ValueDataFieldConstants.DisplayMember, propertyName);

            Func<List<T>> getDataHandler = () => filter == null ? null : filter.Where(p => p.Value != null).Select(p => p.Value).ToList();
            var itemsSource = getDataHandler();
            filterListField.Set(ValueDataFieldConstants.ItemsSource, itemsSource);

            var entitytype = typeof(T);
            string fieldfilter;
            if (entitytype == typeof (Art) || entitytype == typeof (IWB) || entitytype == typeof (OWB))
            {
                var entity = (T) Activator.CreateInstance(entitytype);
                fieldfilter = @"FORMAT(""№ {0} {1} (ид. {2})""," + ValueDataFieldConstants.RowNumberFlag + "," +
                    propertyName + "," + entity.GetPrimaryKeyPropertyName() + ")";
            }
            else
            {
                fieldfilter = @"FORMAT(""№ {0} {1}""," + ValueDataFieldConstants.RowNumberFlag + "," + propertyName + ")";
            }

            filterListField.Set(ValueDataFieldConstants.CustomDisplayMember, fieldfilter);

            Func<Filter<T>> getSelectedValueHandler = () =>
            {
                if (filter == null)
                    return null;
                var value = model[FilterValueListModelPropertyName];
                if (value == null)
                    return null;

                return filter.FirstOrDefault(p => p.Value != null && Equals(value, p.Value.GetProperty(propertyName)));
            };

            while (true)
            {
                //Синхронизируем данные в форме
                if (hasFilterChanged)
                {
                    itemsSource = getDataHandler();
                    filterListField.Set(ValueDataFieldConstants.ItemsSource, itemsSource);
                }
                var filterAny = itemsSource != null && itemsSource.Any();
                footerMenu[0].IsEnabled = footerMenu[1].IsEnabled = filterAny; //<-- -->
                footerMenu[3].IsEnabled = footerMenu[4].IsEnabled = filterAny; //Удаление

                if (viewService.ShowDialogWindow(model, false) != true)
                    return;

                switch (model.MenuResult)
                {
                    case "F3": //Добавить
                        var filterValue = model[FilterModelPropertyName];
                        if (!parameter.Validate(filterValue, viewService, model.PanelCaption, fontSize))
                            continue;

                        if (entitytype == typeof(TE))
                        {
                            if (filterValue != null)
                            {
                                var tecode = (string) filterValue;
                                filterValue = IsTeCodeToUpper.Get(_context) ? tecode.ToUpper() : tecode;
                            }
                        }

                        if (!ValidateValue(filter, filterValue, viewService, model.PanelCaption))
                            continue;

                        if (entitytype == typeof (Art))
                        {
                            if (!GetFilteredEntity(filterName, filter, filterValue, Art.MANDANTIDPropertyName, viewService, model.PanelCaption, ArtNotFoundFormat, fontSize))
                                continue;
                        }
                        else if (entitytype == typeof (IWB))
                        {
                            if (!GetFilteredEntity(filterName, filter, filterValue, IWB.MANDANTIDPropertyName, viewService, model.PanelCaption, IwbNotFoundFormat, fontSize))
                                continue;
                        }
                        else if (entitytype == typeof (OWB))
                        {
                            if (!GetFilteredEntity(filterName, filter, filterValue, OWB.MANDANTIDPropertyName, viewService, model.PanelCaption, OwbNotFoundFormat, fontSize))
                                continue;
                        }
                        else
                        {
                            AddParameterValue(filter, filterValue);
                        }
                        
                        model[FilterModelPropertyName] = null;
                        model[FilterValueListModelPropertyName] = filterValue;
                        hasFilterChanged = true;
                        continue;
                    case "F4": //Удалить
                        if (!filterAny)
                            continue;

                        var selectedValue = getSelectedValueHandler();
                        if (selectedValue == null)
                            continue;

                        if (selectedValue.IsReadOnly)
                        {
                            viewService.ShowDialog(model.PanelCaption, "Удаление значения фильтра запрещено.", MessageBoxButton.OK,
                                MessageBoxImage.Warning, MessageBoxResult.OK, fontSize);
                            continue;
                        }

                        if (DeleteConfirmation(viewService, model.PanelCaption,
                            "Вы уверены, что хотите удалить текущее значение фильтра?", fontSize))
                        {
                            filter.Remove(selectedValue);
                            hasFilterChanged = true;
                        }
                        continue;
                    case "F5": //Удалить всё
                        if (!filterAny)
                            continue;

                        if (DeleteConfirmation(viewService, model.PanelCaption,
                            "Вы уверены, что хотите удалить все значения фильтра?", fontSize))
                        {
                            foreach (var p in filter.Where(p => !p.IsReadOnly).ToArray())
                            {
                                filter.Remove(p);
                                hasFilterChanged = true;
                            }
                        }
                        continue;
                    default:
                        return;
                }
            }
        }

        private DialogSourceViewModel CreateUpdateFilterDialogModel(ValueDataField[] footerMenu, string dialogTitle)
        {
            var result = new DialogSourceViewModel
            {
                PanelCaption = dialogTitle,
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false
            };

            var field = new ValueDataField
            {
                Name = FilterModelPropertyName,
                Caption = "Добавить",
                FieldType = typeof(string),
                IsEnabled = true,
                SetFocus = true
            };
            field.Set(ValueDataFieldConstants.IsDoNotMovedFocusOnNextControl, true);
            AddField(result, field);

            field = new ValueDataField
            {
                Name = FilterValueListModelPropertyName,
                Caption = string.Empty,
                FieldType = typeof(object),
                IsEnabled = true
            };
            field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectControl.ToString());
            field.Set(ValueDataFieldConstants.MaxRowsOnPage, FilterValuesMaxRow);
            AddField(result, field);

            if (footerMenu != null)
            {
                field = new ValueDataField
                {
                    Name = ValueDataFieldConstants.FooterMenu,
                    Caption = ValueDataFieldConstants.FooterMenu,
                    FieldType = typeof (FooterMenu),
                    IsEnabled = true
                };
                field.Set(ValueDataFieldConstants.FooterMenu, footerMenu);
                AddField(result, field);
            }

            result.UpdateSource();
            return result;
        }

        private ValueDataField[] CreateFilterDialogFooterMenuPart1()
        {
            var footerMenu = new List<ValueDataField>();
            var footerMenuItem = new ValueDataField
            {
                Name = "Menu0",
                Caption = "ШК артикула",
                Value = "F1"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu1",
                Caption = "Артикул",
                Value = "F2"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu2",
                Caption = "ТЕ",
                Value = "F3"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu3",
                Caption = "Место",
                Value = "F4"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu4",
                Caption = "Ещё",
                Value = "F5"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu5",
                Caption = "Выйти",
                Value = "Escape"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);
            return footerMenu.ToArray();
        }

        private ValueDataField[] CreateFilterDialogFooterMenuPart2()
        {
            var footerMenu = new List<ValueDataField>();
            var footerMenuItem = new ValueDataField
            {
                Name = "Menu0",
                Caption = "Приход",
                Value = "F1"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu1",
                Caption = "Расход",
                Value = "F2"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu2",
                Caption = "Вх. груз",
                Value = "F3"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu3",
                Caption = "Исх. груз",
                Value = "F4"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu4",
                Caption = "Ещё",
                Value = "F5"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu5",
                Caption = "Выйти",
                Value = "Escape"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);
            return footerMenu.ToArray();
        }

        private ValueDataField[] CreateSelectProductDialogFooterMenu()
        {
            var footerMenu = new List<ValueDataField>();
            var footerMenuItem = new ValueDataField
            {
                Name = "Menu0",
                Caption = "Выбрать",
                Value = "F1"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenuItem.Set(ValueDataFieldConstants.IsNotMenuButton, true);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu1",
                Caption = "Выйти",
                Value = "Escape"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu2",
                Caption = "Фильтр",
                Value = "F2"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu3",
                Caption = "Применить",
                Value = "Enter"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            return footerMenu.ToArray();
        }

        private ValueDataField[] CreateUpdateFilterDialogFooterMenu()
        {
            var footerMenu = new List<ValueDataField>();
            var footerMenuItem = new ValueDataField
            {
                Name = "MenuMovePrevious",
                Caption = "<--",
                Value = "F1"
            };
            footerMenuItem.Set(ValueDataFieldConstants.HotKey2, "Left");
            footerMenuItem.Set(ValueDataFieldConstants.TransferHotKeyToControls, true);
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "MenuMoveNext",
                Caption = "-->",
                Value = "F2"
            };
            footerMenuItem.Set(ValueDataFieldConstants.HotKey2, "Right");
            footerMenuItem.Set(ValueDataFieldConstants.TransferHotKeyToControls, true);
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "MenuAdd",
                Caption = "Добавить",
                Value = "F3"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "MenuDelete",
                Caption = "Удалить",
                Value = "F4"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "MenuDeleteAll",
                Caption = "Удалить всё",
                Value = "F5"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "MenuEsc",
                Caption = "Назад",
                Value = "Escape"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            return footerMenu.ToArray();
        }

        private bool ShowSelectEntityDialog(Type entityType, WMSBusinessObject[] itemsSource, string title, string gridLayout)
        {
            if (itemsSource == null)
                throw new ArgumentNullException("itemsSource");

            var footerMenu = CreateSelectEntityDialogFooterMenu();
            var model = CreateSelectEntityDialogModel(entityType, footerMenu, itemsSource, title, SettingDisplay.LookUp, false, gridLayout);
            footerMenu[0].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() =>
            {
                var id = model[ItemsSourceModelPropertyName];
                if (id == null)
                    return;
                var row = itemsSource.FirstOrDefault(p => Equals(p.GetKey(), id)) as IIsSelected;
                if (row == null)
                    return;

                row.IsSelected = !row.IsSelected;
            }));

            footerMenu[1].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() =>
            {
                foreach (var p in itemsSource.OfType<IIsSelected>())
                {
                    p.IsSelected = true;
                }
            }));

            var viewService = IoC.Instance.Resolve<IViewService>();

            while (true)
            {
                if (viewService.ShowDialogWindow(model, false) != true)
                    return false;

                switch (model.MenuResult)
                {
                    case "Value": //Двойной клик мышки
                    case "Return": //Применить
                        if (!itemsSource.OfType<IIsSelected>().Any(p => p.IsSelected))
                        {
                            ShowError(SelectProductDialogTitle, "Ничего не выбрано. Необходимо выбрать один из элементов фильтра.");
                            continue;
                        }
                        return true;
                    default:
                        return false;
                }
            }
        }

        private DialogSourceViewModel CreateSelectEntityDialogModel(Type entityType, ValueDataField[] footerMenu, WMSBusinessObject[] itemsSource, string title, SettingDisplay displaySetting, bool showAutoFilterRow, string gridLayout)
        {
            var result = new DialogSourceViewModel
            {
                PanelCaption = title,
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
                LayoutValue = gridLayout
            };

            var field = new ValueDataField
            {
                Name = ItemsSourceModelPropertyName,
                Caption = string.Empty,
                FieldType = entityType,
                LabelPosition = "Left",
                IsEnabled = true,
                SetFocus = true,
                CloseDialog = true
            };
            field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
            field.Set(ValueDataFieldConstants.ShowControlMenu, false);
            field.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, false);
            field.Set(ValueDataFieldConstants.ShowAutoFilterRow, showAutoFilterRow);
            field.Set(ValueDataFieldConstants.DoNotActionOnEnterKey, true);
            field.Set(ValueDataFieldConstants.ItemsSource, itemsSource);

            //Получаем поля для грида
            var fieldList = DataFieldHelper.Instance.GetDataFields(entityType, displaySetting);
            var selectField = new DataField
            {
                Name = IsSelectedPropertyName,
                BindingPath = IsSelectedPropertyName,
                Caption = string.Empty,
                EnableEdit = false,
                IsEnabled = true,
                FieldName = IsSelectedPropertyName,
                FieldType = typeof(bool),
                SourceName = IsSelectedPropertyName
            };
            fieldList.Insert(0, selectField);

            field.Set(ValueDataFieldConstants.Fields, fieldList.ToArray());

            if (footerMenu != null)
                field.Set(ValueDataFieldConstants.FooterMenu, footerMenu);

            AddField(result, field);

            result.UpdateSource();
            return result;
        }

        private ValueDataField[] CreateSelectEntityDialogFooterMenu()
        {
            var footerMenu = new List<ValueDataField>();
            var footerMenuItem = new ValueDataField
            {
                Name = "Menu00",
                Caption = "Выбрать",
                Value = "F1"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenuItem.Set(ValueDataFieldConstants.IsNotMenuButton, true);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu01",
                Caption = "Все",
                Value = "F2"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenuItem.Set(ValueDataFieldConstants.IsNotMenuButton, true);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu10",
                Caption = "Выйти",
                Value = "Escape"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu11",
                Caption = "Применить",
                Value = "Enter"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            return footerMenu.ToArray();
        }

        private bool GetFilteredEntity<T>(string filterName, List<Filter<T>> parameters, object filterValue, string mandantIdPropertyName, IViewService viewService, string title, string notFoundMessageFormat, double fontsize) where T : WMSBusinessObject
        {
            var entityType = typeof(T);
            var notFoundMessage = string.Format(notFoundMessageFormat, string.Format("'{0}'", filterValue));
            try
            {
                var filter = GetBaseFilter(entityType, filterValue);
                if (!string.IsNullOrEmpty(mandantIdPropertyName) && _mandantId.HasValue)
                {
                    filter += string.Format(" AND {0} = {1}",
                        SourceNameHelper.Instance.GetPropertySourceName(entityType, mandantIdPropertyName), _mandantId);
                    notFoundMessage = string.Format(notFoundMessageFormat, string.Format("'{0}' для манданта '{1}'", filterValue, _mandantCode));
                }
                T[] entities;
                using (var mgr = GetManager(entityType))
                {
                    entities = mgr.GetFiltered(filter, GetModeEnum.Partial).OfType<T>().ToArray();
                }

                switch (entities.Length)
                {
                    case 0:
                        ShowError(viewService, title, notFoundMessage, fontsize);
                        return false;
                    case 1:
                        AddParameterValue(parameters, entities[0]);
                        return true;
                    default:
                        string gridLayout = null;
                        WMSBusinessObject[] itemsSource = null;
                        if (entityType == typeof(Art))
                        {
                            itemsSource = entities.Select(p => new ArtPl(p as Art)).ToArray();
                            entityType = typeof (ArtPl);
                            gridLayout = @"<?xml version=""1.0"" encoding=""utf-16""?><CustomDataLayoutControl ID=""objectDataLayout"" Orientation=""Vertical""><Element ID=""ItemsSource"" VerticalAlignment=""Stretch"" Label="""" ItemsSource_RclGridControl_RclGridControl=""&lt;XtraSerializer version=&quot;1.0&quot; application=&quot;GridControl&quot;&gt;&#xD;&#xA;  &lt;property name=&quot;#LayoutVersion&quot;&gt;1.0.0.0&lt;/property&gt;&#xD;&#xA;  &lt;property name=&quot;$GridControl&quot; iskey=&quot;true&quot; value=&quot;GridControl&quot;&gt;&#xD;&#xA;    &lt;property name=&quot;GroupCount&quot;&gt;0&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;Columns&quot; iskey=&quot;true&quot; value=&quot;17&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;Item1&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IsSelected&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;UnboundType&quot;&gt;Boolean&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;20&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item2&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;1&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTCODE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;164&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item3&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;2&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;114&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item4&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;MANDANTID&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;66&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item5&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTDESC&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;451&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item6&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTDESCEXT&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item7&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTCOLOR&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item8&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTCOLORTONE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item9&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTSIZE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item10&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTPICKORDER&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item11&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTTEMPMIN&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item12&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTTEMPMAX&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item13&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTLIFETIME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item14&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTDANGERLEVEL&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item15&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTDANGERSUBLEVEL&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item16&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;ARTHOSTREF&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item17&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VFACTORYNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;View&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;AllowColumnFiltering&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AutoWidth&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowGroupPanel&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ColumnChooserState&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;Size&quot;&gt;220,250&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Location&quot;&gt;1185,345&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowIndicator&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowFilterPanelMode&quot;&gt;Never&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ItemsSourceErrorInfoShowMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;NavigationStyle&quot;&gt;Cell&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowEditing&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowSearchPanelCloseButton&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;FormatConditions&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;      &lt;property name=&quot;SearchPanelHighlightResults&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;IsRowCellMenuEnabled&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowCascadeUpdate&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowPerPixelScrolling&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ScrollAnimationMode&quot;&gt;Linear&lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;TotalSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;SelectionMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;SortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;MRUFilters&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummarySortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;  &lt;/property&gt;&#xD;&#xA;&lt;/XtraSerializer&gt;"" /><AvailableItems /></CustomDataLayoutControl>";
                        }
                        else if (entityType == typeof(IWB))
                        {
                            itemsSource = entities.Select(p => new IwbPl(p as IWB)).ToArray();
                            entityType = typeof(IwbPl);
                            gridLayout = @"<?xml version=""1.0"" encoding=""utf-16""?><CustomDataLayoutControl ID=""objectDataLayout"" Orientation=""Vertical""><Element ID=""ItemsSource"" VerticalAlignment=""Stretch"" Label="""" ItemsSource_RclGridControl_RclGridControl=""&lt;XtraSerializer version=&quot;1.0&quot; application=&quot;GridControl&quot;&gt;&#xD;&#xA;  &lt;property name=&quot;#LayoutVersion&quot;&gt;1.0.0.0&lt;/property&gt;&#xD;&#xA;  &lt;property name=&quot;$GridControl&quot; iskey=&quot;true&quot; value=&quot;GridControl&quot;&gt;&#xD;&#xA;    &lt;property name=&quot;GroupCount&quot;&gt;0&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;Columns&quot; iskey=&quot;true&quot; value=&quot;28&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;Item1&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IsSelected&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;UnboundType&quot;&gt;Boolean&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;20&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item2&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;1&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBID&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;41&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item3&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;2&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;126&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item4&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBPRIORITY&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item5&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBINDATEPLAN&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Default&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item6&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBCMRNUMBER&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item7&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBCMRDATE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Default&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item8&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBHOSTREF&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item9&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBDESC&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item10&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBSENDER_NAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item11&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBPAYER_NAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item12&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IWBRECIPIENT_NAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item13&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;STATUSCODE_R_NAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;82&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item14&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCARGOIWB&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;41&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item15&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VEXTERNALTRAFFIC&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item16&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VEXTERNALTRAFFICDRIVERFIO&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item17&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VINTERNALTRAFFICFACTARRIVED&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item18&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VINTERNALTRAFFICFACTDEPARTED&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item19&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VMANDANTCODE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;66&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item20&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VOWBNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item21&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VFACTORYNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item22&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCARGOIWBLOADBEGIN&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item23&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCARGOIWBLOADEND&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item24&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCUSTOMPARAMSVALUE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item25&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VIWBINVOICENUMBER&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item26&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VIWBINVOICEDATE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Default&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item27&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCOUNTIWBPOS&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;114&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item28&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;7&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VINVNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;View&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;AllowColumnFiltering&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AutoWidth&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowGroupPanel&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ColumnChooserState&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;Size&quot;&gt;220,250&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Location&quot;&gt;1185,345&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowIndicator&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowFilterPanelMode&quot;&gt;Never&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ItemsSourceErrorInfoShowMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;NavigationStyle&quot;&gt;Cell&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowEditing&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowSearchPanelCloseButton&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;FormatConditions&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;      &lt;property name=&quot;SearchPanelHighlightResults&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;IsRowCellMenuEnabled&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowCascadeUpdate&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowPerPixelScrolling&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ScrollAnimationMode&quot;&gt;Linear&lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;TotalSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;SelectionMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;SortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;MRUFilters&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummarySortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;  &lt;/property&gt;&#xD;&#xA;&lt;/XtraSerializer&gt;"" /><AvailableItems /></CustomDataLayoutControl>";
                        }
                        else if (entityType == typeof(OWB))
                        {
                            itemsSource = entities.Select(p => new OwbPl(p as OWB)).ToArray();
                            entityType = typeof(OwbPl);
                            gridLayout = @"<?xml version=""1.0"" encoding=""utf-16""?><CustomDataLayoutControl ID=""objectDataLayout"" Orientation=""Vertical""><Element ID=""ItemsSource"" VerticalAlignment=""Stretch"" Label="""" ItemsSource_RclGridControl_RclGridControl=""&lt;XtraSerializer version=&quot;1.0&quot; application=&quot;GridControl&quot;&gt;&#xD;&#xA;  &lt;property name=&quot;#LayoutVersion&quot;&gt;1.0.0.0&lt;/property&gt;&#xD;&#xA;  &lt;property name=&quot;$GridControl&quot; iskey=&quot;true&quot; value=&quot;GridControl&quot;&gt;&#xD;&#xA;    &lt;property name=&quot;GroupCount&quot;&gt;0&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;Columns&quot; iskey=&quot;true&quot; value=&quot;31&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;Item1&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;IsSelected&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;UnboundType&quot;&gt;Boolean&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;20&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item2&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;1&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBID&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;48&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item3&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;2&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;126&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item4&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBPRIORITY&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item5&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBOUTDATEPLAN&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Default&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item6&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBDESC&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item7&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBPROXYNUMBER&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item8&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBPROXYDATE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Default&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item9&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBHOSTREFDATE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Default&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item10&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBHOSTREF&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item11&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;OWBGROUP&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item12&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;5&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCARGOOWB&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;37&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item13&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VEXTERNALTRAFFIC&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item14&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VEXTERNALTRAFFICDRIVERFIO&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item15&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VINTERNALTRAFFICFACTARRIVED&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item16&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VINTERNALTRAFFICFACTDEPARTED&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item17&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;3&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VMANDANTCODE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;66&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item18&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;4&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VSTATUSCODENAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;102&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item19&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VOWBRECIPIENTNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item20&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VOWBPRODUCTNEEDNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item21&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VOWBRESERVTYPENAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item22&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VOWBCRITPLNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item23&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VFACTORYNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item24&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCARGOOWBLOADBEGIN&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item25&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCARGOOWBLOADEND&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item26&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VADDRESSBOOKCOMPLEX&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item27&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VOWBPAYERNAME&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item28&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VROUTENUMBER&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item29&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;Value&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VROUTEDATE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Default&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item30&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;6&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VCOUNTOWBPOS&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;114&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;Item31&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;VisibleIndex&quot;&gt;7&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Visible&quot;&gt;false&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ColumnFilterMode&quot;&gt;DisplayText&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AllowColumnFiltering&quot;&gt;False&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Name&quot;&gt;VOWNERCODE&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;AutoFilterCondition&quot;&gt;Contains&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;GridRow&quot;&gt;0&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;ActualWidth&quot;&gt;NaN&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;View&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;      &lt;property name=&quot;AllowColumnFiltering&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AutoWidth&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowGroupPanel&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ColumnChooserState&quot; isnull=&quot;true&quot; iskey=&quot;true&quot;&gt;&#xD;&#xA;        &lt;property name=&quot;Size&quot;&gt;220,250&lt;/property&gt;&#xD;&#xA;        &lt;property name=&quot;Location&quot;&gt;1185,350&lt;/property&gt;&#xD;&#xA;      &lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowIndicator&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowFilterPanelMode&quot;&gt;Never&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ItemsSourceErrorInfoShowMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;NavigationStyle&quot;&gt;Cell&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowEditing&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ShowSearchPanelCloseButton&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;FormatConditions&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;      &lt;property name=&quot;SearchPanelHighlightResults&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;IsRowCellMenuEnabled&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowCascadeUpdate&quot;&gt;true&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;AllowPerPixelScrolling&quot;&gt;false&lt;/property&gt;&#xD;&#xA;      &lt;property name=&quot;ScrollAnimationMode&quot;&gt;Linear&lt;/property&gt;&#xD;&#xA;    &lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;TotalSummary&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;SelectionMode&quot;&gt;None&lt;/property&gt;&#xD;&#xA;    &lt;property name=&quot;SortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;MRUFilters&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;    &lt;property name=&quot;GroupSummarySortInfo&quot; iskey=&quot;true&quot; value=&quot;0&quot; /&gt;&#xD;&#xA;  &lt;/property&gt;&#xD;&#xA;&lt;/XtraSerializer&gt;"" /><AvailableItems /></CustomDataLayoutControl>";
                        }
                        else
                        {
                            return false;
                        }

                        if (ShowSelectEntityDialog(entityType, itemsSource, string.Format("Выбор для фильтра '{0}'", filterName), gridLayout))
                        {
                            foreach (var p in itemsSource.OfType<IIsSelected>().Where(p => p.IsSelected))
                            {
                                AddParameterValue(parameters, p);
                            }

                            return true;
                        }
                        return false;
                }
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);

                _log.Debug(ex);
                _log.Warn(message);

                ShowError(viewService, title,
                    string.Format("Ошибки при получении данных {0}", message), fontsize);
                return false;
            }
        }

        private static bool ValidateNullvalue(object value, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (value == null)
                return false;
            if (type == typeof(string) && value is string)
            {
                return !((string)value).IsNullOrEmptyAfterTrim();
            }

            return true;
        }

        private static void AddField(List<ValueDataField> fields, ValueDataField field)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            if (field == null)
                throw new ArgumentNullException("field");

            field.FieldName = field.Name;
            field.SourceName = field.Name;
            fields.Add(field);
        }

        private static void AddField(DialogSourceViewModel model, ValueDataField field)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (field == null)
                throw new ArgumentNullException("field");

            AddField(model.Fields, field);
        }

        private static Parameter GetParameter(Type key)
        {
            if (!FilterParameters.ContainsKey(key))
                throw new DeveloperException("Property name is not defined for type '{0}'.", key);
            return FilterParameters[key];
        }

        private bool ValidateValue<T>(IEnumerable<Filter<T>> parameterList, object value, IViewService viewService, string title) where T : WMSBusinessObject
        {
            var key = typeof(T);
            var propertyName = GetParameter(key).PropertyName;

            if (parameterList.Any(p => p.Value != null && Equals(p.Value.GetProperty(propertyName), value)))
            {
                ShowError(viewService, title,
                    string.Format("Значение '{0}' уже существует в списке значений фильтра.", value),
                    FontSize.Get(_context));
                return false;
            }

            return true;
        }

        private IBaseManager GetManager(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var mto = IoC.Instance.Resolve<IManagerForObject>();
            var mgrType = mto.GetManagerByTypeName(type.Name);
            if (mgrType == null)
                throw new DeveloperException(string.Format("Unknown source type '{0}'.", type.Name));

            var managerInstance = IoC.Instance.Resolve(mgrType, null) as IBaseManager;
            if (managerInstance == null)
                throw new DeveloperException(string.Format("Can't resolve IBaseManager by '{0}'.", mgrType.Name));
            return managerInstance;
        }

        private void AddParameterValue<T>(List<Filter<T>> parameterList, object value) where T : WMSBusinessObject
        {
            if (parameterList == null)
                throw new ArgumentNullException("parameterList");
            if (value == null)
                throw new ArgumentNullException("value");

            T newitem;
            if (value is T)
            {
                newitem = (T) value;
            }
            else
            {
                var key = typeof (T);
                var propertyName = GetParameter(key).PropertyName;

                newitem = (T) Activator.CreateInstance(typeof (T));
                newitem.SetProperty(propertyName, value);
            }

            parameterList.Add(new Filter<T> {Value = newitem});
        }

        private string GetBaseFilter(Type type, object value)
        {
             if (type == null)
                throw new ArgumentNullException("type");
            if (value == null)
                throw new ArgumentNullException("value");

            var parameter = GetParameter(type);
            var propertyType = parameter.PropertyType;
            var filter = string.Format("{0} = {1}{2}{1}",
                SourceNameHelper.Instance.GetPropertySourceName(propertyType, parameter.PropertyName),
                propertyType == typeof(string) ? "'" : null,
                value);
            return filter;
        }

        private List<Filter<T>> DistinctParameter<T>(IEnumerable<T> parameters, bool isReadOnly) where T : WMSBusinessObject
        {
            if (parameters == null)
                return new List<Filter<T>>();

            var filters =
                parameters.Where(p => p != null)
                    .Select(p => new Filter<T> {Value = p, IsReadOnly = isReadOnly})
                    .ToList();
            return filters;
        }

        //private List<Filter<T>> DistinctParameter<T>(ICollection<Filter<T>> filters) where T : WMSBusinessObject
        //{
        //    var parameter = GetParameter(typeof(T));
        //    var propertyName = parameter.PropertyName;

        //    return filters != null && filters.Any()
        //        ? new List<Filter<T>>(parameter.PropertyType == typeof(string)
        //            ? filters.Where(p => p.Value != null && !string.IsNullOrEmpty(p.Value.GetProperty<string>(propertyName)))
        //                .Distinct(new FilterComparer<T>())
        //            : filters.Distinct(new FilterComparer<T>()))
        //        : new List<Filter<T>>();
        //}

        private IList<object> GetParameterValues<T>(ICollection<Filter<T>> parameters, bool usePropertyName = false) where T : WMSBusinessObject
        {
            if (parameters != null)
            {
                var filtervalues = parameters.Where(p => p.Value != null).ToArray();
                if (filtervalues.Any())
                {
                    if (usePropertyName)
                    {
                        var propertyName = GetParameter(typeof (T)).PropertyName;
                        return filtervalues.Select(p => p.Value.GetProperty(propertyName)).Distinct().ToList();
                    }

                    return filtervalues.Select(p => p.Value.GetKey()).Distinct().ToList();
                }
            }

            return new List<object>();
        }

        private static void ShowError(IViewService viewService, string title, string message, double fontSize)
        {
            viewService.ShowDialog(title, message, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, fontSize);
        }

        private void ShowError(string title, string message)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            ShowError(viewService, title, message, FontSize.Get(_context));
        }

        private static bool DeleteConfirmation(IViewService viewService, string title, string message, double fontSize)
        {
            var result = viewService.ShowDialog(title, message, MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No, fontSize);

            return result == MessageBoxResult.Yes;
        }

        private static string GetFilterIndicator(ICollection filters)
        {
            var count = 0;
            if (filters != null)
                count = filters.Count;
            return string.Format("[{0}]", count <= 0 ? "-" : count.ToString(CultureInfo.InvariantCulture));
        }
        #endregion .  Methods  .

        private class Parameter
        {
            public Parameter(string propertyName, Type propertyType, string validateNullvalueMessage)
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentNullException("propertyName");
                if (propertyType == null)
                    throw new ArgumentNullException("propertyType");
                if (string.IsNullOrEmpty(validateNullvalueMessage))
                    throw new ArgumentNullException("validateNullvalueMessage");

                PropertyName = propertyName;
                PropertyType = propertyType;
                ValidateNullvalueMessage = validateNullvalueMessage;
            }

            public string PropertyName { get; private set; }
            public Type PropertyType { get; private set; }
            public string ValidateNullvalueMessage { get; private set; }
            public string ValidateTypeMessageFormat { get; set; }
            public Func<object, IViewService, string, string, double, bool> ValidateType { get; set; } 

            public bool Validate(object value, IViewService viewService, string title, double fontSize)
            {
                if (!ValidateNullvalue(value, PropertyType))
                {
                    ShowError(viewService, title, ValidateNullvalueMessage, fontSize);
                    return false;
                }

                if (ValidateType != null)
                    return ValidateType(value, viewService, title, ValidateTypeMessageFormat, fontSize);

                return true;
            }
        }

        //private class FilterComparer<T> : IEqualityComparer<Filter<T>> where T : WMSBusinessObject
        //{
        //    public bool Equals(Filter<T> x, Filter<T> y)
        //    {
        //        return new ParameterComparer<T>().Equals(x.Value, y.Value);
        //    }

        //    public int GetHashCode(Filter<T> obj)
        //    {
        //        if (ReferenceEquals(obj, null))
        //            return 0;

        //        if (obj.Value == null)
        //            return obj.IsReadOnly.GetHashCode();

        //        var propertyValue = obj.Value.GetKey();
        //        return propertyValue == null ? 0 : propertyValue.GetHashCode();
        //    }
        //}

        //private class ParameterComparer<T> : IEqualityComparer<T> where T : WMSBusinessObject
        //{
        //    public bool Equals(T x, T y)
        //    {
        //        if (ReferenceEquals(x, y))
        //            return true;

        //        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
        //            return false;

        //        return Equals(x.GetKey(), y.GetKey());
        //    }

        //    public int GetHashCode(T obj)
        //    {
        //        if (ReferenceEquals(obj, null))
        //            return 0;

        //        var propertyValue = obj.GetKey();
        //        return propertyValue == null ? 0 : propertyValue.GetHashCode();
        //    }
        //}

        private class Filter<T> where T : WMSBusinessObject
        {
            public T Value { get; set; }
            public bool IsReadOnly { get; set; }
        }


        [SysObjectName("Product")]
        public class ProductPl : Product
        {
            public ProductPl()
            {
            }

            public ProductPl(Product product)
            {
                if (product == null)
                    return;

                try
                {
                    SuspendNotifications();
                    Copy(product, this);
                    AcceptChanges();
                }
                finally
                {
                    ResumeNotifications();
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if (_isSelected == value)
                        return;
                    _isSelected = value;
                    OnPropertyChanged(IsSelectedPropertyName);
                }
            }
        }

        [SysObjectName("Art")]
        public class ArtPl : Art, IIsSelected
        {
            public ArtPl()
            {
            }

            public ArtPl(Art art)
            {
                if (art == null)
                    return;

                try
                {
                    SuspendNotifications();
                    Copy(art, this);
                    AcceptChanges();
                }
                finally
                {
                    ResumeNotifications();
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if (_isSelected == value)
                        return;
                    _isSelected = value;
                    OnPropertyChanged(IsSelectedPropertyName);
                }
            }
        }

        [SysObjectName("IWB")]
        public class IwbPl : IWB, IIsSelected
        {
            public IwbPl()
            {

            }

            public IwbPl(IWB iwb)
            {
                if (iwb == null)
                    return;

                try
                {
                    SuspendNotifications();
                    Copy(iwb, this);
                    AcceptChanges();
                }
                finally
                {
                    ResumeNotifications();
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if (_isSelected == value)
                        return;
                    _isSelected = value;
                    OnPropertyChanged(IsSelectedPropertyName);
                }
            }
        }

         [SysObjectName("OWB")]
        public class OwbPl : OWB, IIsSelected 
        {
            public OwbPl()
            {
            }

            public OwbPl(OWB owb)
            {
                if (owb == null)
                    return;

                try
                {
                    SuspendNotifications();
                    Copy(owb, this);
                    AcceptChanges();
                }
                finally
                {
                    ResumeNotifications();
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if (_isSelected == value)
                        return;
                    _isSelected = value;
                    OnPropertyChanged(IsSelectedPropertyName);
                }
            }
        }

        public interface IIsSelected
        {
            bool IsSelected { get; set; }
        }
    }
}
