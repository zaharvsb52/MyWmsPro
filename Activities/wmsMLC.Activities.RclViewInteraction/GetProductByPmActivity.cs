using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class GetProductByPmActivity : NativeActivity
    {
        #region .  Arguments  .
        /// <summary>
        /// Коллекция товаров для группировки.
        /// </summary>
        [DisplayName(@"Входная коллекция товара")]
        [Description("Коллекция товара на вход обработки")]
        public InArgument<List<Product>> ProductList { get; set; }

        /// <summary>
        /// Операция.
        /// </summary>
        [DisplayName(@"Операция")]
        [Description("Код операции, по которой будет производится группировка товара")]
        public InArgument<string> OperationCode { get; set; }

        [DisplayName(@"Вывод грида")]
        [Description("Признак вывода грида (если запись 1)")]
        public InArgument<bool> NeedShowSelectGrid { get; set; }

        /// <summary>
        /// Вид списка полей для списка товара.
        /// </summary>
        [DisplayName(@"Вид списка полей для списка товара")]
        public SettingDisplay DisplaySetting { get; set; }

        /// <summary>
        /// Список полей грида. В качестве разделителя используется ','.
        /// </summary>
        [DisplayName(GridFieldsDisplayName)]
        [Description("Список полей для списка товара. В качестве разделителя используется ','.")]
        [DefaultValue(GridFieldsDefault)]
        public InArgument<string> GridFields { get; set; }

        [DisplayName(@"Результат диалога")]
        public OutArgument<bool> DialogResult { get; set; }

        /// <summary>
        /// Группа товаров.
        /// </summary>
        [DisplayName(@"Сгруппированная коллекция товара на выходе")]
        [Description("Сгруппированная коллекция товара на выходе, в соответствии с выбором пользователя")]
        public OutArgument<List<Product>> OutGroupProductList { get; set; }

        /// <summary>
        /// Ид. выбранных товаров.
        /// </summary>
        [DisplayName(@"Коллекция товара на выходе")]
        [Description("Коллекция товара на выходе, в соответствии с выбором пользователя")]
        public OutArgument<List<Product>> OutProductList { get; set; }
        #endregion  .  Arguments  .

        #region .  Fields&Properties  .
        //private readonly List<string> _objNameGrid = new List<string>() { "PRODUCTCOUNTSKU", "SKUID_R_NAME", "VARTNAME", "VARTDESC", "VMEASURENAME" };
        private const string GridFieldsDefault = "VARTNAME, PRODUCTCOUNTSKU, SKUID_R_NAME, VARTDESC, VMEASURENAME";
        private const string ProductListModelPropertyName = "ProductList";
        private const string GridFieldsDisplayName = @"Список полей для списка товара";

        private DialogSourceViewModel _mainModel;
        private List<string> _objNameGrid;
        private decimal? _selectedPrd;
        #endregion  .  Fields&Properties  .

        #region .  Methods  .
        public GetProductByPmActivity()
        {
            DisplayName = "ТСД: Группировка товара по менеджеру товара";
            GridFields = GridFieldsDefault;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, ProductList,
                type.ExtractPropertyName(() => ProductList));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode,
                type.ExtractPropertyName(() => OperationCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, NeedShowSelectGrid,
                type.ExtractPropertyName(() => NeedShowSelectGrid));
            ActivityHelpers.AddCacheMetadata(collection, metadata, GridFields,
                type.ExtractPropertyName(() => GridFields));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResult,
                type.ExtractPropertyName(() => DialogResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OutGroupProductList,
                type.ExtractPropertyName(() => OutGroupProductList));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OutProductList,
                type.ExtractPropertyName(() => OutProductList));
 
            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            const string isnullerrorformat = "Свойство '{0}' должно быть задано.";

            var gridFieldsStr = GridFields.Get(context);
            if (string.IsNullOrEmpty(gridFieldsStr))
                throw new DeveloperException(isnullerrorformat, GridFieldsDisplayName);

            _objNameGrid = new List<string>(gridFieldsStr.Split(new []{' ', ','}, StringSplitOptions.RemoveEmptyEntries));

            var productList = ProductList.Get(context);
            var operationCode = OperationCode.Get(context);
            var needShowGreed = NeedShowSelectGrid.Get(context);

            try
            {
                var productsList = new List<Tuple<string, Product>>();
                var userGrpPrd = new List<Product>();

                //получим настройки менеджера товара
                using (var mgr = IoC.Instance.Resolve<IPMConfigManager>())
                {
                    foreach (var prd in productList)
                    {
                        var groupKeys = new StringBuilder();
                        //обязательная группировка по skuid
                        groupKeys.AppendFormat("{0}={1}_", Product.SKUIDPropertyName, prd.SKUID);

                        var confs = mgr.GetPMConfigByParamListByProductId(prd.ProductId, operationCode, null).ToArray();
                        //формируем группы по товару и настройке менеджера товара
                        foreach (var pmConfig in confs.OrderBy(x => x.ObjectName_r))
                        {
                            var prdprop = prd.GetProperty(pmConfig.ObjectName_r) ?? string.Empty;
                            _objNameGrid.Add(pmConfig.ObjectName_r);

                            groupKeys.AppendFormat("{0}={1}_", pmConfig.ObjectName_r, prdprop);
                        }

                        productsList.Add(new Tuple<string, Product>(groupKeys.ToString(), prd));
                    }
                }

                //формируем коллекцию товара для отображения + сумма по группам
                var grpPoductsList = productsList.GroupBy(p => p.Item1, v => v.Item2).ToArray();
                foreach (var gr in grpPoductsList)
                {
                    var product = (Product) gr.First().Clone();
                    product.ProductCountSKU = gr.Sum(p => p.ProductCountSKU);
                    userGrpPrd.Add(product);
                }
                
                //если всего 1 группа и needShowGreed == false, грид не показываем
                if (grpPoductsList.Length == 1 && !needShowGreed)
                {
                    OutGroupProductList.Set(context, userGrpPrd);
                    OutProductList.Set(context, grpPoductsList[0].ToList());
                    return;
                }

                var dialogRet = ShowMain(userGrpPrd);

                //выбираем товар, в соответствии с выбором пользователя
                var outGroupProductList = new List<Product>();
                var outProductList = new List<Product>();
                if (dialogRet && _selectedPrd != null)
                {
                    outGroupProductList = userGrpPrd.Where(p => p.GetKey<decimal>() == _selectedPrd).ToList();

                    var grkey = (from gr in grpPoductsList where gr.Any(p => p.GetKey<decimal>() == _selectedPrd) select gr.Key).FirstOrDefault();
                    outProductList = grpPoductsList.Where(g => g.Key == grkey).SelectMany(p => p).ToList();
                }

                OutGroupProductList.Set(context, outGroupProductList);
                OutProductList.Set(context, outProductList);
                DialogResult.Set(context, dialogRet);
            }
            catch (Exception ex)
            {
                var message = "Ошибка при получении настроек менеджера товара. " +
                              ExceptionHelper.GetErrorMessage(ex, false);
                ShowWarning(message);
            }
        }

        private DialogSourceViewModel GetMainModel(List<Product> prdList)
        {
            if (_mainModel == null)
            {
                _mainModel = new DialogSourceViewModel
                {
                    PanelCaption = "Выбор товара",
                    IsMenuVisible = false,
                    FontSize = 15
                };

                var prdLst = new ValueDataField
                {
                    Name = ProductListModelPropertyName,
                    Caption = string.Empty,
                    FieldType = typeof (Product),
                    LabelPosition = "None",
                    IsEnabled = true,
                    SetFocus = true,
                    CloseDialog = true
                };

                prdLst.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
                prdLst.Set(ValueDataFieldConstants.ShowControlMenu, true);
                prdLst.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, false);
                prdLst.Set(ValueDataFieldConstants.ShowAutoFilterRow, false);
                prdLst.Set(ValueDataFieldConstants.DoNotActionOnEnterKey, false);
                prdLst.Set(ValueDataFieldConstants.ItemsSource, prdList);
                prdLst.Set(ValueDataFieldConstants.CloseDialogOnSelectedItemChanged, true);

                var grdFields = GetGridFields(prdLst.FieldType, DisplaySetting);

                prdLst.Set(ValueDataFieldConstants.Fields, grdFields.ToArray());

                _mainModel.Fields.Add(prdLst);
            }
            else
            {
                _mainModel.GetField(ProductListModelPropertyName).Set(ValueDataFieldConstants.ItemsSource, prdList);
            }

            _mainModel.UpdateSource();

            return _mainModel;
        }

        private bool ShowMain(List<Product> prdList)
        {
            var model = GetMainModel(prdList);
            string menuResult;

            if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                return false;

            while (true)
            {
                switch (model.MenuResult)
                {
                    case "Value":
                        var id = model[ProductListModelPropertyName] as decimal?;
                        if (!id.HasValue)
                        {
                            ShowWarning("Не выбран товар. Выберите товар.");
                            continue;
                        }
                        _selectedPrd = id.Value;
                        model.UpdateSource();
                        break;
                    default:
                        return false;
                }
                return true;
            }
        }

        private static void ShowWarning(string message, string caption = "Размещение")
        {
            GetViewService()
                .ShowDialog(caption, message, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        }

        private static IViewService GetViewService()
        {
            return IoC.Instance.Resolve<IViewService>();
        }

        private List<DataField> GetGridFields(Type type, SettingDisplay displaySetting)
        {
            var fieldList = DataFieldHelper.Instance.GetDataFields(type, displaySetting);
            if (_objNameGrid == null)
                return new List<DataField>(fieldList);

            return
                _objNameGrid.Distinct()
                    .Select(propertyName => fieldList.FirstOrDefault(p => p.Name.EqIgnoreCase(propertyName)))
                    .Where(field => field != null)
                    .ToList();
        }

        #endregion .  Methods  .
    }
}