using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using DevExpress.Xpf.Core.Native;
using log4net;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Factory;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.Activities.RclViewInteraction
{
    public enum RclInvTaskMode
    {
        GROUP,
        PLACE,
        SURPLUS
    }

    public class RclInvTaskActivity : NativeActivity<string>
    {
        #region .  Constants  .

        private const string SkuIdRPropertyName = "SKUID_R";

        private const string SkuIdPropertyName = "SKUID";

        private const string InvTaskGroupIdPropertyName = "INVTASKGROUPID_R";

        private const string InvTaskStepIdPropertyName = "INVTASKSTEPID_R";

        private const string Count2SkuPropertyName = "INVTASKCOUNT2SKU";

        private const string TeTypeCodePropertyName = "TETYPECODE";

        private const string TeTypeCodeRPropertyName = "TETYPECODE_R";

        private const string CountPropertyName = "INVTASKCOUNT";

        private const string BarCodePropertyName = "BARCODE";

        private const string PlaceCodeRPropertyName = "PLACECODE_R";

        private const string PlaceCodePropertyName = "PLACECODE";

        private const string TeCodePropertyName = "INVTASKTECODE";

        private const string ArtDescPropertyName = "VARTDESC";

        private const string TEType2SKUFilterTemplate =
            "TETYPECODE IN (SELECT S2T.TETYPECODE_R FROM WMSSKU2TTE S2T WHERE S2T.SKUID_R = {0})";

        private const int MinCaptionLength = 6;
        private const int MaxCaptionLength = 12;

        #endregion .  Constants  .

        #region .  Fields  .

        private decimal _mandantId;

        private Place _place;

        private double _fontSize;

        private bool _piece;

        #endregion

        #region .  Properties  .

        [DisplayName(@"Режим")]
        public RclInvTaskMode Mode { get; set; }

        [DisplayName(@"Поля")]
        public InArgument<IEnumerable<ValueDataField>> Fields { get; set; }

        [DisplayName(@"Поля MustSet (inner activity use)")]
        public InOutArgument<Dictionary<decimal, List<ValueDataField>>> CacheMustSetFields { get; set; }

        [DisplayName(@"Место")]
        public InArgument<string> PlaceCode { get; set; }

        [DisplayName(@"Мандант")]
        public InArgument<decimal> MandantId { get; set; }

        [DisplayName(@"Подтверждать SKU (по группе)")]
        public InArgument<bool> AskSku { get; set; }

        [DisplayName(@"Задание")]
        public InOutArgument<InvTask> InvTaskObj { get; set; }

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Поштучный ввод (по месту)")]
        [DefaultValue(false)]
        public InArgument<bool> Piece { get; set; }

        #endregion

        public RclInvTaskActivity()
        {
            DisplayName = "ТСД: Форма инвентаризации";
        }

        #region .  Methods  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Fields, type.ExtractPropertyName(() => Fields));
            ActivityHelpers.AddCacheMetadata(collection, metadata, CacheMustSetFields,
                type.ExtractPropertyName(() => CacheMustSetFields));
            ActivityHelpers.AddCacheMetadata(collection, metadata, PlaceCode, type.ExtractPropertyName(() => PlaceCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MandantId, type.ExtractPropertyName(() => MandantId));
            ActivityHelpers.AddCacheMetadata(collection, metadata, AskSku, type.ExtractPropertyName(() => AskSku));
            ActivityHelpers.AddCacheMetadata(collection, metadata, InvTaskObj,
                type.ExtractPropertyName(() => InvTaskObj));
            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Piece, type.ExtractPropertyName(() => Piece));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            // получаем входные параметры
            var invTask = InvTaskObj.Get(context);
            var cacheMustSetFields = CacheMustSetFields.Get(context);
            var fields = Fields.Get(context).Select(i => i.Clone()).Cast<ValueDataField>().ToArray();
            var placeCode = PlaceCode.Get(context);
            var askSku = AskSku.Get(context);

            _mandantId = MandantId.Get(context);
            _fontSize = FontSize.Get(context);
            _piece = Piece.Get(context);

            if (string.IsNullOrEmpty(placeCode))
                throw new OperationException("Не указан код места инвентаризации.");

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Place>>())
                _place = mgr.Get(placeCode);

            if (_place == null)
                throw new OperationException("Место с кодом '{0}' не существует.", placeCode);

            string result = null;

            switch (Mode)
            {
                case RclInvTaskMode.GROUP:
                    result = ShowByGroupView(ref invTask, fields, askSku, ref cacheMustSetFields);
                    break;
                case RclInvTaskMode.PLACE:
                    result = ShowByPlaceView(ref invTask, fields, false, ref cacheMustSetFields);
                    break;
                case RclInvTaskMode.SURPLUS:
                    result = ShowByPlaceView(ref invTask, fields, true, ref cacheMustSetFields);
                    break;
            }
            InvTaskObj.Set(context, invTask);
            CacheMustSetFields.Set(context, cacheMustSetFields);
            Result.Set(context, result);
        }

        private string ShowByGroupView(ref InvTask invTask, ValueDataField[] fields, bool askSku, ref Dictionary<decimal, List<ValueDataField>> cacheMustSetFields)
        {
            var skuAccepted = false;
            ValueDataField barCodeField;
            ValueDataField countField;
            var skuName = invTask.GetProperty("VSKUNAME");
            var skuId = invTask.GetProperty<decimal>(SkuIdRPropertyName);
            var invTaskGroupId = invTask.GetProperty<decimal>(InvTaskGroupIdPropertyName);

            var model = new DialogSourceViewModel
            {
                //PanelCaption = string.Format("Укажите кол-во товара '{0}'", skuName),
                PanelCaption = "Укажите кол-во товара",
                FontSize = _fontSize,
                IsMenuVisible = false,
            };

            var modelFields = new List<ValueDataField>();
            var defaultFields = GetDefaultFields(fields).ToList();

            if (cacheMustSetFields.All(i => i.Key != skuId))
            {
                //Группа заданий еще не закеширована, кешируем
                cacheMustSetFields =
                    GetMustSetFieldsFiltered(
                        string.Format(
                            " skuid in (select tsk.skuid_r from wmsinvtask tsk where tsk.invtaskgroupid_r = {0})",
                            invTaskGroupId));
            }

            //Поищем настройки среди кеша, для группы заданий и ску
            var commonMustSetFields = cacheMustSetFields.FirstOrDefault(x => x.Key == skuId);
            if (commonMustSetFields.Value != null)
            {
                defaultFields.AddRange(commonMustSetFields.Value);
            }

            // прячем SKU
            //var skuField = defaultFields.First(i => i.Name == SkuIdRPropertyName);
            //defaultFields.Remove(skuField);

            // получим описание артикула
            var artDescField = defaultFields.First(i => i.Name == ArtDescPropertyName);
            using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                var sku = mgr.Get(skuId, GetModeEnum.Partial);
                artDescField.Value = sku.GetProperty<string>(ArtDescPropertyName);
            }

            barCodeField = defaultFields.First(i => i.Name == BarCodePropertyName);
            // если не надо запросить ШК SKU
            if (!askSku)
                defaultFields.Remove(barCodeField);

            countField = defaultFields.First(i => i.Name == CountPropertyName);

            // если не пришла настройка, то спрячем ТЕ
            if (fields.All(i => i.Name != TeCodePropertyName))
            {
                var teCodeField = defaultFields.First(i => i.Name == TeCodePropertyName);
                defaultFields.Remove(teCodeField);
            }

            // если не пришла настройка, то спрячем Тип ТЕ
            if (fields.All(i => i.Name != TeTypeCodeRPropertyName))
            {
                var teTypeCodeField = defaultFields.First(i => i.Name == TeTypeCodeRPropertyName);
                defaultFields.Remove(teTypeCodeField);
            }

            // проставим значения и фильтры
            foreach (var f in defaultFields)
            {
                if (f.Name == CountPropertyName || !f.Visible)
                    continue;

                if (!invTask.ContainsProperty(f.Name))
                    continue;

                f.IsEnabled = false;
                var prop = invTask.GetProperty(f.Name);
                if (prop != null)
                    f.Value = prop;

                if (!string.IsNullOrEmpty(f.LookupCode))
                {
                    if (f.Value == null)
                        f.LookupFilterExt = string.Format("{0} is null", f.SourceName);
                    else
                        f.LookupFilterExt = string.Format("{0} = {1}", f.SourceName,
                            (new object[] { typeof(string), typeof(Guid) }).Contains(f.FieldType)
                                ? "'" + f.Value + "'"
                                : f.Value);
                }
            }

            modelFields.AddRange(defaultFields);

            var footerMenu = new ValueDataField()
            {
                Name = "MENU",
                Caption = "Меню",
                FieldName = "MENU",
                SourceName = "MENU",
                FieldType = typeof(IFooterMenu),
                IsEnabled = true
            };

            // добавляем меню
            modelFields.Add(footerMenu);

            // выставляем поля для модели
            model.Fields = modelFields;

            var escapeMenuItem = new ValueDataField()
            {
                Name = "ESCAPE",
                Caption = "Выйти",
                FieldName = "ESCAPE",
                SourceName = "ESCAPE",
                IsEnabled = true,
                Value = "Escape"
            };
            escapeMenuItem.Set(ValueDataFieldConstants.Row, 0);
            escapeMenuItem.Set(ValueDataFieldConstants.Column, 0);

            var enterMenuItem = new ValueDataField()
            {
                Name = "ENTER",
                Caption = "Ввод",
                FieldName = "ENTER",
                SourceName = "ENTER",
                IsEnabled = true,
                Value = "Enter"
            };
            enterMenuItem.Set(ValueDataFieldConstants.Row, 0);
            enterMenuItem.Set(ValueDataFieldConstants.Column, 1);

            var footerMenuItems = new List<ValueDataField> { escapeMenuItem, enterMenuItem };

            var menuField = model.Fields.First(i => i.Name == "MENU");
            menuField.Set(ValueDataFieldConstants.FooterMenu, footerMenuItems.ToArray());
            model.UpdateSource();

            skuAccepted = !askSku;

            try
            {
                while (true)
                {
                    if (askSku)
                        barCodeField.IsEnabled = !skuAccepted;

                    barCodeField.SetFocus = !skuAccepted;
                    countField.SetFocus = skuAccepted;

                    var result = ShowViewModel(model);
                    // пропускаем группу
                    if (result == false)
                        return "SKIPGROUP";

                    if (!skuAccepted)
                    {
                        var barcode = model[BarCodePropertyName].To<string>();
                        if (string.IsNullOrEmpty(barcode))
                        {
                            ShowMessage("Ошибка", "Укажите ШК SKU!", true);
                            continue;
                        }
                        // поищим в ШК
                        using (var mgr = IoC.Instance.Resolve<IBaseManager<Barcode>>())
                            skuAccepted =
                                mgr.GetFiltered(
                                    string.Format(
                                        "BARCODE2ENTITY = 'SKU' and BARCODEKEY = '{0}'and BARCODEVALUE = '{1}'",
                                        invTask.GetProperty(SkuIdRPropertyName), barcode),
                                    FilterHelper.GetAttrEntity<Barcode>(new[] { Barcode.BarcodeEntityPropertyName }))
                                    .Any();
                        // попробуем найти по имени артикула, если не нашли по ШК
                        if (!skuAccepted)
                        {
                            using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
                                skuAccepted =
                                    mgr.GetFiltered(
                                        string.Format(
                                            "SKUID = {0} and exists(select 1 from wmsart where upper(wmsart.artname)=upper('{1}'))",
                                            invTask.GetProperty(SkuIdRPropertyName), barcode),
                                        FilterHelper.GetAttrEntity<SKU>(SKU.ArtCodePropertyName)).Any();
                        }

                        if (!skuAccepted)
                        {
                            model[BarCodePropertyName] = null;
                            ShowMessage("Ошибка",
                                string.Format("Введенный ШК '{0}' не соответствует ед.уч. '{1}'", barcode, skuName),
                                true);
                            continue;
                        }

                        if (skuAccepted && model[CountPropertyName] == null)
                            continue;
                    }

                    var count = (decimal?)model[CountPropertyName];
                    if (count == null)
                    {
                        ShowMessage("Ошибка", "Укажите количество товара!", true);
                        continue;
                    }

                    invTask.SetProperty(InvTask.INVTASKCOUNTPropertyName, count.Value);

                    return "FIX";
                }
            }
            finally
            {
                model.Dispose();
            }
        }

        private Dictionary<decimal, List<ValueDataField>> GetMustSetFieldsFiltered(string filter)
        {
            SKU[] skuInvTaskArray;
            var valueDataFields = new Dictionary<decimal, List<ValueDataField>>();

            using (var mgrSku = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                skuInvTaskArray = mgrSku.GetFiltered(filter,
                    FilterHelper.GetAttrEntity<SKU>(SKU.SKUIDPropertyName, SKU.ArtCodePropertyName)).ToArray();
            }

            using (var mgrBpProc = IoC.Instance.Resolve<IBPProcessManager>())
            {
                foreach (var skuItem in skuInvTaskArray.Distinct())
                {
                    var pmConfigs =
                        mgrBpProc.GetPMConfigByParamListByArtCode(skuItem.ArtCode, "OP_INV_CREATE", "MUST_SET")
                            .ToArray();
                    if (!pmConfigs.Any()) continue;
                    var valueFields = CreateValueDataFieldByPmConfig(pmConfigs);
                    valueDataFields.Add(skuItem.SKUID, valueFields);
                }
            }

            return valueDataFields;
        }

        private List<ValueDataField> CreateValueDataFieldByPmConfig(IEnumerable<PMConfig> pmConfigs)
        {
            var valueField = new ValueDataField();
            var valueFieldLst = new List<ValueDataField>();

            foreach (var pmConfig in pmConfigs)
            {
                switch (pmConfig.ObjectName_r)
                {
                    case "PRODUCTINPUTDATE":
                        valueField = new ValueDataField()
                        {
                            Caption = "Дата приемки",
                            SourceName = "PRODUCTINPUTDATE",
                            IsRequired = true,
                            Name = "INVTASKPRODUCTINPUTDATE",
                            FieldType = typeof(DateTime),
                            IsEnabled = true
                        };
                        break;
                    case "PRODUCTEXPIRYDATE":
                        valueField = new ValueDataField()
                        {
                            Caption = "Срок годности",
                            LabelPosition = "Top",
                            SourceName = "PRODUCTEXPIRYDATE",
                            IsRequired = true,
                            Name = "INVTASKEXPIRYDATE",
                            FieldType = typeof(DateTime),
                            IsEnabled = true
                        };
                        break;
                    case "PRODUCTBATCH":
                        valueField = new ValueDataField()
                        {
                            Caption = "Партия",
                            SourceName = "PRODUCTBATCH",
                            Name = "INVTASKBATCH",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "PRODUCTLOT":
                        valueField = new ValueDataField()
                        {
                            Caption = "Лот",
                            SourceName = "PRODUCTLOT",
                            Name = "INVTASKLOT",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "PRODUCTSERIALNUMBER":
                        valueField = new ValueDataField()
                        {
                            Caption = "Сер.номер",
                            SourceName = "PRODUCTSERIALNUMBER",
                            Name = "INVTASKSERIALNUMBER",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "PRODUCTCOLOR":
                        valueField = new ValueDataField()
                        {
                            Caption = "Цвет",
                            SourceName = "PRODUCTCOLOR",
                            Name = "INVTASKCOLOR",
                            IsRequired = true,
                            LookupCode = "PARTNER_COLOR",
                            FieldType = typeof(String),
                            IsEnabled = false
                        };
                        break;
                    case "PRODUCTTONE":
                        valueField = new ValueDataField()
                        {
                            Caption = "Тон",
                            SourceName = "PRODUCTTONE",
                            Name = "INVTASKTONE",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "PRODUCTSIZE":
                        valueField = new ValueDataField()
                        {
                            Caption = "Размер",
                            SourceName = "PRODUCTSIZE",
                            Name = "INVTASKSIZE",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "FACTORYID_R":
                        valueField = new ValueDataField()
                        {
                            Caption = "Фабрика",
                            SourceName = "FACTORYID",
                            Name = "FACTORYID_R",
                            IsRequired = true,
                            FieldType = typeof(decimal?),
                            LookupCode = "FACTORY_FACTORYID",
                            IsEnabled = false
                        };
                        break;
                    case "PRODUCTBATCHCODE":
                        valueField = new ValueDataField()
                        {
                            Caption = "Batch-код",
                            SourceName = "PRODUCTBATCHCODE",
                            Name = "INVTASKBATCHCODE",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "PRODUCTBOXNUMBER":
                        valueField = new ValueDataField()
                        {
                            Caption = "Номер короба",
                            SourceName = "PRODUCTBOXNUMBER",
                            Name = "INVTASKBOXNUMBER",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "TECODE_R":
                        valueField = new ValueDataField()
                        {
                            Caption = "ТЕ",
                            SourceName = "TECODE_R",
                            Name = "INVTASKTECODE",
                            IsRequired = true,
                            FieldType = typeof(String),
                            IsEnabled = true
                        };
                        break;
                    case "QLFCODE_R":
                        valueField = new ValueDataField()
                        {
                            Caption = "Квалификация",
                            LabelPosition = "Top",
                            SourceName = "QLFCODE",
                            Name = "QLFCODE_R",
                            IsRequired = true,
                            FieldType = typeof(String),
                            LookupCode = "QLF_QLFCODE",
                            IsEnabled = false
                        };
                        break;
                    case "QLFDETAILCODE_R":
                        valueField = new ValueDataField()
                        {
                            Caption = "Дет. КВЛФ",
                            SourceName = "QLFDETAILCODE",
                            Name = "QLFDETAILCODE_R",
                            IsRequired = true,
                            FieldType = typeof(String),
                            LookupCode = "QLFDETAIL_QLFDETAILCODE",
                            IsEnabled = false
                        };
                        break;
                    default:
                        break;
                }
                valueFieldLst.Add(valueField);
            }

            return valueFieldLst;
        }

        private string ShowByPlaceView(ref InvTask invTask, IEnumerable<ValueDataField> fields, bool surplus, ref Dictionary<decimal, List<ValueDataField>> cacheMustSetFields)
        {
            var checkPoints = new Dictionary<string, Dictionary<string, object>> { {
                    "\r\n 1. start ShowByPlaceView", new Dictionary<string, object>
                    {
                        {"invTask", invTask.InvTaskId},
                        {"fields", fields.Select(f => f.Name)},
                        {"surplus", surplus},
                        {"cacheMustSetFields", cacheMustSetFields.Select(f => f.Value.Select(v => v.Name))}
                    }}};

            try
            {
                // если не задали объект
                if (invTask == null)
                    throw new OperationException("Не указана группа заданий по месту. Обратитесь в службу поддержки.");

                var invTaskGroupId = invTask.GetProperty<decimal>(InvTaskGroupIdPropertyName);

                var model = new DialogSourceViewModel
                {
                    PanelCaption = "Укажите товар",
                    FontSize = _fontSize,
                    IsMenuVisible = false,
                };

                var modelFields = new List<ValueDataField>();

                // получим поля по умолчанию вместе с настройками менеджера
                var defaultFields = GetDefaultFields(fields, true, surplus);

                checkPoints.Add("\r\n 2. GetDefaultFields", new Dictionary<string, object>
                    {
                        {"defaultFields", defaultFields.Select(f => f.Name)},
                        {"byPlace", true}
                    });

                // если режим по месту и из настроек не пришло ТЕ, то прячем его
                var ignoreTe = !surplus && !fields.Any(f => f.Name == TeCodePropertyName);
                if (ignoreTe)
                {
                    var teField = defaultFields.First(f => f.Name == TeCodePropertyName);
                    teField.Visible = false;
                    teField.IsRequired = false;
                }

                var barCodeField = defaultFields.First(i => i.Name == BarCodePropertyName);
                var countField = defaultFields.First(i => i.Name == CountPropertyName);

                // Если признак поштучно
                if (_piece)
                {
                    countField.Value = 1;
                    countField.Visible = true;
                    countField.IsEnabled = false;
                }
                else
                {
                    countField.Value = null;
                    countField.Visible = true;
                }

                // не прячим поле место
                var placeField = defaultFields.First(i => i.Name == PlaceCodeRPropertyName);
                placeField.Visible = true;
                placeField.Value = _place.GetKey<string>();

                var ignoreTeType = !surplus && !fields.Any(f => f.Name == TeTypeCodePropertyName);

                // прячем тип ТЕ для мест
                if (ignoreTeType)
                {
                    var teTypeField = defaultFields.First(i => i.Name == TeTypeCodeRPropertyName);
                    teTypeField.Visible = false;
                    teTypeField.IsRequired = false;
                }

                checkPoints.Add("\r\n 3. main fields", new Dictionary<string, object>
                    {
                        {"ignoreTe", ignoreTe},
                        {"ignoreTeType", ignoreTeType},
                        {"_piece", _piece},
                        {"placeField.Value", placeField.Value}
                    });

                string teTypeCode = null;
                // найдем ТЕ на месте
                string teCode = null;
                using (var mgr = IoC.Instance.Resolve<IBaseManager<TE>>())
                {
                    var teList =
                        mgr.GetFiltered(string.Format("TECURRENTPLACE = '{0}'", _place.PlaceCode),
                            FilterHelper.GetAttrEntity<TE>(TE.TECodePropertyName, TE.TETypeCodePropertyName)).ToArray();
                    // если ТЕ одно, то используем
                    if (teList.Length == 1)
                    {
                        if (!ignoreTe)
                            teCode = teList[0].TECode;
                        if (!ignoreTeType)
                            teTypeCode = teList[0].TETypeCode;
                    }
                }
                // укажем ТЕ если есть
                if (!ignoreTe && !string.IsNullOrEmpty(teCode))
                {
                    var teField = defaultFields.First(i => i.Name == TeCodePropertyName);
                    teField.Value = teCode;
                }

                checkPoints.Add("\r\n 4. found TE", new Dictionary<string, object>
                    {
                        {"teCode", teCode},
                        {"teTypeCode", teTypeCode}
                    });

                // укажем тип ТЕ если есть
                if (surplus && !string.IsNullOrEmpty(teTypeCode))
                {
                    var teTypeField = defaultFields.First(i => i.Name == TeTypeCodeRPropertyName);
                    teTypeField.Value = teTypeCode;
                }

                modelFields.AddRange(defaultFields);

                // создадим меню
                var footerMenu = new ValueDataField()
                {
                    Name = "MENU",
                    Caption = "Меню",
                    FieldName = "MENU",
                    SourceName = "MENU",
                    FieldType = typeof(IFooterMenu),
                    IsEnabled = true
                };

                // добавляем меню
                modelFields.Add(footerMenu);

                // выставляем поля для модели
                model.Fields = modelFields;

                var escapeMenuItem = new ValueDataField()
                {
                    Name = "ESCAPE",
                    Caption = "Выйти",
                    FieldName = "ESCAPE",
                    SourceName = "ESCAPE",
                    IsEnabled = true,
                    Value = "Escape"
                };
                escapeMenuItem.Set(ValueDataFieldConstants.Row, 0);
                escapeMenuItem.Set(ValueDataFieldConstants.Column, 0);

                var enterMenuItem = new ValueDataField()
                {
                    Name = "ENTER",
                    Caption = "Ввод",
                    FieldName = "ENTER",
                    SourceName = "ENTER",
                    IsEnabled = true,
                    Value = "Enter"
                };
                enterMenuItem.Set(ValueDataFieldConstants.Row, 0);
                enterMenuItem.Set(ValueDataFieldConstants.Column, 1);

                var closeTaskMenuItem = new ValueDataField()
                {
                    Name = "ACCEPT",
                    Caption = "Закрыть",
                    FieldName = "ACCEPT",
                    SourceName = "ACCEPT",
                    IsEnabled = true,
                    Value = "F8"
                };
                closeTaskMenuItem.Set(ValueDataFieldConstants.Row, 1);
                closeTaskMenuItem.Set(ValueDataFieldConstants.Column, 0);

                var footerMenuItems = new List<ValueDataField> { escapeMenuItem, enterMenuItem };
                // данная кнопка есть только у инв. по месту
                if (!surplus)
                    footerMenuItems.Add(closeTaskMenuItem);

                // получим лукапные поля
                var lookUpFields =
                    modelFields.Where(i => !string.IsNullOrEmpty(i.LookupCode) && i.Visible && i.Name != "PLACECODE_R")
                        .Select(p => p.Clone())
                        .Cast<ValueDataField>()
                        .ToArray();
                foreach (var f in lookUpFields)
                {
                    f.IsEnabled = true;
                    if (f.Name == "QLFCODE_R")
                        f.Caption = "КВЛФ";
                }
                footerMenuItems.AddRange(lookUpFields);

                checkPoints.Add("\r\n 5. lookUpFields", new Dictionary<string, object>
                    {
                        {"lookUpFields", lookUpFields.Select(f => f.Name)}
                    });

                var menuField = model.Fields.First(i => i.Name == "MENU");
                menuField.Set(ValueDataFieldConstants.FooterMenu, footerMenuItems.ToArray());
                model.UpdateSource();
                
                try
                {
                    while (true)
                    {
                        var skuIdObj = model[SkuIdRPropertyName];

                        // проставим фильтры
                        foreach (var f in defaultFields.Where(f => !string.IsNullOrEmpty(f.LookupCode)))
                        {
                            var value = model[f.Name];
                            if (value == null)
                                f.LookupFilterExt = string.Format("{0} is null", f.SourceName);
                            else
                                f.LookupFilterExt = string.Format("{0} = {1}", f.SourceName,
                                    new object[] { typeof(string), typeof(Guid) }.Contains(f.FieldType)
                                        ? "'" + value + "'"
                                        : value);
                        }

                        if (model[SkuIdPropertyName] == null && model[SkuIdRPropertyName] == null &&
                            model[BarCodePropertyName] == null)
                        {
                            barCodeField.SetFocus = skuIdObj == null;
                            countField.SetFocus = skuIdObj != null;
                        }

                        // чистим ШК
                        model[BarCodePropertyName] = null;

                        var result = ShowViewModel(model);

                        checkPoints.Add(string.Format("\r\n ---6. {0} ShowViewModel", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                            {
                                {"result", result},
                                {"model.MenuActionName", model.MenuActionName}
                            });

                        // пропускаем группу или просто выходим
                        if (result == false)
                            return surplus ? null : "SKIP";

                        switch (model.MenuActionName)
                        {
                            // подтверждаем задание
                            case "ENTER":

                                var barcode = model[BarCodePropertyName];
                                // если указали ШК или пустой ШК и не указана SKU
                                if (barcode != null ||
                                    (barcode == null && model[SkuIdRPropertyName] == null))
                                {
                                    var skuId = SearchSku(barcode.To<string>());

                                    checkPoints.Add(string.Format("\r\n 7. {0} SearchSku", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"barcode", barcode.To<string>()},
                                            {"skuId", skuId}
                                        });

                                    // обработаем смену SKU
                                    var isNeedToRun = ProcessMustSetFields(surplus, ref cacheMustSetFields, skuId, invTaskGroupId, model, defaultFields, menuField);

                                    checkPoints.Add(string.Format("\r\n 8. {0} ProcessMustSetFields", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"isNeedToRun", isNeedToRun},
                                            {"skuId", skuId},
                                            {"invTaskGroupId", invTaskGroupId},
                                            {"cacheMustSetFields", cacheMustSetFields.Values.Select(v => v.Select(vv => vv.Name))}
                                        });

                                    var skuChanged = ProcessSkuChanged(model, skuId, surplus);

                                    checkPoints.Add(string.Format("\r\n 9. {0} ProcessSkuChanged", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"skuChanged", skuChanged},
                                            {"isNeedToRun", isNeedToRun}
                                        });

                                    if (!skuChanged || isNeedToRun)
                                        continue;
                                }

                                var notAssign =
                                    model.Fields.FirstOrDefault(
                                        x => x.IsRequired && string.IsNullOrEmpty(x.LookupCode) && model[x.Name] == null);

                                if (notAssign != null)
                                {
                                    checkPoints.Add(string.Format("\r\n 9,5. {0} notAssign", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"notAssign", notAssign.Name}
                                        });
                                    model.Fields.ForEach(dataField => dataField.SetFocus = false);
                                    notAssign.SetFocus = true;
                                    continue;
                                }

                                var count = _piece ? 1 : (decimal?)model[CountPropertyName];
                                checkPoints.Add(string.Format("\r\n 10. {0} count", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"count", count}
                                        });
                                if (count == null)
                                {
                                    ShowMessage("Ошибка", "Укажите количество товара!", true);
                                    continue;
                                }

                                // валидация остальных полей
                                var errorMessages = new StringBuilder();

                                foreach (var f1 in model.Fields.Where(x => !string.IsNullOrEmpty(x.LookupCode)))
                                {
                                    if (f1.IsRequired && model[f1.Name] == null)
                                        errorMessages.AppendFormat("Не заполнено обязательное поле '{0}'{1}", f1.Caption, Environment.NewLine);
                                }
                                if (errorMessages.Length > 0)
                                {
                                    ShowMessage("Ошибка", errorMessages.ToString(), true);
                                    continue;
                                }

                                teCode = model[TeCodePropertyName].To<string>();
                                // проверим/получим тип ТЕ
                                if (!ignoreTeType)
                                {
                                    teTypeCode = GetTeTypeCode(teCode, model[TeTypeCodeRPropertyName].To<string>(), model[SkuIdRPropertyName].To<decimal>());

                                    checkPoints.Add(string.Format("\r\n 10,5. {0} teTypeCode", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"teTypeCode", teTypeCode}
                                        });

                                    if (string.IsNullOrEmpty(teTypeCode))
                                    {
                                        var message = !string.IsNullOrEmpty(teCode)
                                            ? string.Format("Для ТЕ '{0}' не указан тип!", teCode)
                                            : "Не указан тип ТЕ!";
                                        ShowMessage("Ошибка", message, true);
                                        continue;
                                    }
                                    // выставляем тип ТЕ
                                    invTask.SetProperty(TeTypeCodeRPropertyName, teTypeCode);
                                }

                                var reorderInvTask = new InvTask();
                                reorderInvTask.SetProperty(InvTaskGroupIdPropertyName, invTask.GetProperty(InvTaskGroupIdPropertyName));
                                reorderInvTask.SetProperty(InvTaskStepIdPropertyName, invTask.GetProperty(InvTaskStepIdPropertyName));
                                reorderInvTask.SetProperty(TeTypeCodeRPropertyName, invTask.GetProperty(TeTypeCodeRPropertyName));

                                // заполняем поля задания (исключаем тип ТЕ)
                                foreach (var modelField in model.Fields.Where(i => i.Name != TeTypeCodeRPropertyName))
                                {
                                    // выставим значения только существующих артрибутов объекта
                                    if (reorderInvTask.ContainsProperty(modelField.Name))
                                    {
                                        checkPoints.Add(string.Format("\r\n 11. {0} reorderInvTask.SetProperty({1})", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), modelField.Name), new Dictionary<string, object>
                                        {
                                            {modelField.Name, model[modelField.Name]}
                                        });

                                        reorderInvTask.SetProperty(modelField.Name, model[modelField.Name]);
                                    }
                                    else checkPoints.Add(string.Format("\r\n 11. {0} !reorderInvTask.ContainsProperty({1})", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), modelField.Name), new Dictionary<string, object>{});

                                    //TODO: зачем вообще нужен reorderInvTask? 
                                    //TODO: почему invTask = reorderInvTask; происходит именно здесь, внутри цикла прохода по полям модели?
                                    invTask = reorderInvTask;
                                }
                                return surplus ? "FIX" : "FIXBUF";

                            case "ACCEPT":
                                return "ACCEPT";

                            // наверное это лукап
                            default:
                                var field = model.Fields.FirstOrDefault(i => i.Name == model.MenuActionName);

                                if (field == null)
                                {
                                    checkPoints.Add(string.Format("\r\n 6,5. {0} field = null", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"model.MenuActionName", model.MenuActionName}
                                        });
                                }
                                else
                                {
                                    checkPoints.Add(string.Format("\r\n 7. {0} someField", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"field.Name", field.Name},
                                            {"field.LookupCode", field.LookupCode}
                                        });
                                }

                                if (field == null && string.IsNullOrEmpty(field.LookupCode)) //TODO: скорее всего тут баг, т.к. field.LookupCode даст System.NullReferenceException если field == null
                                    continue;

                                // получим информацию по лукапу
                                var lookupInfo = LookupHelper.GetLookupInfo(field.LookupCode);
                                checkPoints.Add(
                                    lookupInfo == null
                                        ? string.Format("\r\n 7,5. {0} lookupInfo == null", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture))
                                        : string.Format("\r\n 7,5. {0} lookupInfo.ItemType == {1}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), lookupInfo.ItemType),
                                    new Dictionary<string, object> {});

                                // создадим экземпляр объекта и проверим существоание привязки к манаданту
                                var entity = (WMSBusinessObject)Activator.CreateInstance(lookupInfo.ItemType);
                                var filter = entity.ContainsProperty("MANDANTID")
                                    ? string.Format("MANDANTID = {0}", _mandantId)
                                    : null;
                                // фильтр привязки типа ТЕ к SKU
                                if (field.Name == TeTypeCodeRPropertyName)
                                    filter = model[SkuIdRPropertyName] == null
                                        ? "1 != 1"
                                        : string.Format(TEType2SKUFilterTemplate, model[SkuIdRPropertyName]);

                                checkPoints.Add(string.Format("\r\n 8. {0} filter + lookup", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                    {
                                        {"filter", filter},
                                        {"method", field.Name == SkuIdRPropertyName ? "ShowFilterView" : "ShowLookUpGrid" }
                                    });

                                // отобразим лукап
                                var obj = field.Name == SkuIdRPropertyName
                                    ? ShowFilterView(field, filter)
                                    : ShowLookUpGrid(field, filter);

                                if (obj != null)
                                {
                                    checkPoints.Add(string.Format("\r\n 9. {0} obj != null", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object> { });
                                    model[field.Name] = SerializationHelper.ConvertToTrueType(obj, field.FieldType);
                                    // если менялось SKU
                                    if (field.Name == SkuIdRPropertyName)
                                    {
                                        checkPoints.Add(string.Format("\r\n 10. {0} field.Name == SkuIdRPropertyName", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"(decimal?)obj", (decimal?)obj}
                                        });

                                        ProcessMustSetFields(surplus, ref cacheMustSetFields, (decimal?)obj, invTaskGroupId, model, defaultFields, menuField);

                                        checkPoints.Add(string.Format("\r\n 11. {0} ProcessMustSetFields", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>
                                        {
                                            {"cacheMustSetFields", cacheMustSetFields.Values.Select(f => f.Select(v => v.Name))}
                                        });

                                        ProcessSkuChanged(model, (decimal?)obj, surplus);

                                        checkPoints.Add(string.Format("\r\n 11. {0} ProcessSkuChanged", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>{});
                                    }
                                    else
                                        checkPoints.Add(string.Format("\r\n 10. {0} field.Name != SkuIdRPropertyName", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>{});
                                }
                                else
                                    checkPoints.Add(string.Format("\r\n 9. {0} obj == null", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)), new Dictionary<string, object>{});

                                break;
                        }
                    }
                }
                finally
                {
                    model.Dispose();
                }
            }
            catch (Exception exception)
            {
                var logger = LogManager.GetLogger("wmsMLC.RCL.Main.Module");
                logger.Info(checkPoints);
                throw;
            }
        }

        private bool ProcessMustSetFields(bool surplus, ref Dictionary<decimal, List<ValueDataField>> cacheMustSetFields, object skuIdObj, decimal invTaskGroupId, DialogSourceViewModel model, IEnumerable<ValueDataField> defaultFields, ValueDataField menuField)
        {
            var isNeedToFeelMustSet = false;
            if (skuIdObj != null)
            {
                var skuIdFromObj = skuIdObj as decimal?;
                var mustSetFieldsFromCache = new List<ValueDataField>();

                mustSetFieldsFromCache = ProcessCacheMustSetFields(ref cacheMustSetFields, skuIdFromObj,
                    invTaskGroupId);

                model.Fields.Clear();
                model.Fields.AddRange(defaultFields);
                var menuFIelds = menuField.Get<ValueDataField[]>(ValueDataFieldConstants.FooterMenu).ToList();

                //перерисовка полей и меню с учетом must_set
                if (mustSetFieldsFromCache != null && mustSetFieldsFromCache.Any())
                {
                    foreach (var f in mustSetFieldsFromCache.Where(
                            i => string.IsNullOrEmpty(i.LookupCode) && i.Visible && i.Name != "PLACECODE_R" &&
                                menuFIelds.All(x => x.Name != i.Name)))
                    {
                        f.IsEnabled = true;
                    }

                    model.Fields.AddRange(mustSetFieldsFromCache);
                    isNeedToFeelMustSet = true;

                    // получим лукапные поля
                    var mustSetLookUpFields =
                        mustSetFieldsFromCache.Where(
                            i =>
                                !string.IsNullOrEmpty(i.LookupCode) && i.Visible && i.Name != "PLACECODE_R" &&
                                menuFIelds.All(x => x.Name != i.Name))
                            .Select(p => p.Clone())
                            .Cast<ValueDataField>()
                            .ToArray();

                    foreach (var f in mustSetLookUpFields)
                    {
                        f.IsEnabled = true;
                        if (f.Name == "QLFCODE_R")
                            f.Caption = "КВЛФ";
                    }
                    menuFIelds.AddRange(mustSetLookUpFields);
                }

                menuField.Set(ValueDataFieldConstants.FooterMenu, menuFIelds.ToArray());
                model.Fields.Add(menuField);
                model.UpdateSource();
                ProcessSkuChanged(model, skuIdFromObj, surplus);
                var setFocusField =
                    model.Fields.OrderBy(i => i.Order).FirstOrDefault(
                        x => x.IsRequired && string.IsNullOrEmpty(x.LookupCode) && model[x.Name] == null);
                if (setFocusField != null)
                {
                    foreach (var valueDataField in model.Fields)
                    {
                        valueDataField.SetFocus = false;
                    }
                    setFocusField.SetFocus = true;
                }
                else
                {
                    model.GetField(CountPropertyName).SetFocus = true;
                }
            }
            return isNeedToFeelMustSet;
        }

        private List<ValueDataField> ProcessCacheMustSetFields(ref Dictionary<decimal, List<ValueDataField>> cacheMustSetFields, decimal? skuIdFromObj, decimal invTaskGroupId)
        {
            if (skuIdFromObj == null)
                return null;

            List<ValueDataField> mustSetFieldsFromCache;
            if (!cacheMustSetFields.TryGetValue(skuIdFromObj.Value, out mustSetFieldsFromCache))
            {
                //кешируем настройки must_set
                if (cacheMustSetFields == null)
                    cacheMustSetFields =
                        GetMustSetFieldsFiltered(
                            string.Format(
                                " skuid in (select tsk.skuid_r from wmsinvtask tsk where tsk.invtaskgroupid_r = {0})",
                                invTaskGroupId));

                if (!cacheMustSetFields.TryGetValue(skuIdFromObj.Value, out mustSetFieldsFromCache))
                {
                    var dictMustSet =
                        GetMustSetFieldsFiltered(string.Format("skuid = {0}", skuIdFromObj.Value));

                    mustSetFieldsFromCache =
                        dictMustSet
                            .Select(x => x.Value)
                            .FirstOrDefault();
                    cacheMustSetFields.AddRange(dictMustSet);
                }
            }
            return mustSetFieldsFromCache;
        }

        private string GetTeTypeCode(string teCode, string teTypeCode, decimal skuId)
        {
            TE existTe = null;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<TE>>())
                existTe = mgr.Get(teCode,
                    FilterHelper.GetAttrEntity<TE>(new[] { TE.TECodePropertyName, TE.TETypeCodePropertyName }));

            // ТЕ существует, берем у него тип
            if (existTe != null)
                return existTe.TETypeCode;

            // если ТЕ не существует и тип задан, то вернем его
            if (existTe == null && !string.IsNullOrEmpty(teTypeCode))
                return teTypeCode;

            //попробуем определить тип ТЕ для манданта по префексу
            //INFO: была фильтрация по манданту (пока оставил)
            //var filter = string.Format("TETYPECODE IN (SELECT M.TETYPECODE_R FROM WMSTETYPE2MANDANT M WHERE M.PARTNERID_R = {0})", _mandantId);
            //фильтруем по привязке SKU к типам ТЕ
            var filter = string.Format(TEType2SKUFilterTemplate, skuId);
            teTypeCode = BPH.DetermineTeTypeCodeByTeCode(teCode, filter);
            if (!string.IsNullOrEmpty(teTypeCode))
                return teTypeCode;
            // предложим выбрать
            return ShowTeTypeModel(filter);
        }

        private string ShowTeTypeModel(string filter)
        {
            const string layoutSuffix = "0B429B0F-DC15-49BF-B595-6AF2855A6F50";
            var teType = string.Empty;
            var factory = IoC.Instance.Resolve<IObjectListFactory>();
            var model = factory.CreateModel<TEType>("Укажите тип ТЕ", filter, null, _fontSize, layoutSuffix);
            var vs = IoC.Instance.Resolve<IViewService>();
            var result = vs.ShowDialogWindow(model, true, false, "50%", "50%");
            if (result != true)
                return null;
            var selectable = model as ISelectable;
            if (selectable == null)
                throw new OperationException("Модель '{0}' не реализует интерфейс ISelectable", model);
            var selKeys = selectable.GetSelectedKeys();
            if (selKeys != null)
                teType = selKeys[0].To<string>();
            var customdisp = model as ICustomDisposable;
            if (customdisp != null)
            {
                customdisp.SuspendDispose = false;
                customdisp.Dispose();
            }
            return teType;
        }

        private bool ProcessSkuChanged(DialogSourceViewModel model, decimal? skuId, bool surplus)
        {
            if (skuId != null)
            {
                model[SkuIdRPropertyName] = skuId;
                SKU sku = null;
                // получим тип ТЕ по умолчанию и проставим кол-во в SKU
                using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
                    sku = mgr.Get(skuId, GetModeEnum.Partial);
                if (sku == null)
                {
                    // очистим SKU, кол-во в SKU и тип ТЕ
                    model[SkuIdRPropertyName] = null;
                    model[Count2SkuPropertyName] = null;
                    // описание артикула
                    model[ArtDescPropertyName] = null;
                    if (surplus)
                        model[TeTypeCodeRPropertyName] = null;
                    ShowMessage("Ошибка", string.Format("Единица учета с кодом '{0}' не найдена!", skuId.Value), true);
                    return false;
                }

                // описание артикула
                model[ArtDescPropertyName] = sku.Get<string>(ArtDescPropertyName);

                model[Count2SkuPropertyName] = sku.GetProperty<double>(SKU.SKUCountPropertyName);
                // найдем тип ТЕ
                if (surplus)
                {
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU2TTE>>())
                    {
                        var sku2TeList =
                            mgr.GetFiltered(string.Format("SKUID_R = {0} AND SKU2TTEDEFAULT = 1", skuId),
                                FilterHelper.GetAttrEntity<SKU2TTE>(new[]
                                {SKU2TTE.SKU2TTEIDPropertyName, SKU2TTE.TETypeCodePropertyName})).ToArray();
                        if (sku2TeList.Length > 0)
                            model[TeTypeCodeRPropertyName] = sku2TeList[0].TETypeCode;
                    }
                }
            }
            else if (model[SkuIdRPropertyName] == null)
            {
                ShowMessage("Ошибка", "Укажите единицу учета!", true);
                return false;
            }
            // указано ли количество уже
            if (model[CountPropertyName] == null ||
                (model.GetField(CountPropertyName).SetFocus == false && !_piece &&
                 (decimal?)model[CountPropertyName] == 1))
                return false;
            return true;
        }

        private decimal? SearchSku(string barcode)
        {
            string filter;
            var field = new ValueDataField
            {
                Name = SkuIdRPropertyName,
                FieldName = SkuIdRPropertyName,
                SourceName = SkuIdRPropertyName,
                FieldType = typeof(decimal?),
                Caption = "Ед.уч.",
                LookupCode = "SKU_SKUID"
            };

            // если ШК пустой, то отображается список SKU по месту (товар на месте инвентаризации)
            if (string.IsNullOrEmpty(barcode))
            {
                filter =
                    string.Format(
                        "SKUID IN (SELECT P.SKUID_R FROM WMSPRODUCT P INNER JOIN WMSTE T ON T.TECODE = P.TECODE_R WHERE T.TECURRENTPLACE = '{0}')",
                        _place.GetKey());
                field.Caption = string.Format("Ед.уч. на месте '{0}'", _place.PlaceName);
            }
            else
            {
                filter =
                    string.Format(
                        "MANDANTID = {0} AND SKUID IN (SELECT BAR.BARCODEKEY FROM WMSBARCODE BAR WHERE BAR.BARCODE2ENTITY = 'SKU' AND BAR.BARCODEVALUE = '{1}')",
                        _mandantId, barcode);
                using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
                {
                    var skuList =
                        mgr.GetFiltered(filter, FilterHelper.GetAttrEntity<SKU>(new[] { SKU.SKUIDPropertyName }))
                            .ToArray();
                    // нашли одно SKU
                    if (skuList.Length == 1)
                        return skuList[0].GetKey<decimal>();
                    // если найдено несколько SKU, то отображается список найденных
                    // иначе отображается список всех SKU манданта инвентаризации
                    if (skuList.Length > 1)
                        field.Caption = string.Format("Ед.уч. с ШК '{0}'", barcode);
                    else
                    {
                        filter = string.Format("MANDANTID = {0} ", _mandantId);
                        return ShowFilterView(field, filter);
                    }
                }
            }
            return (decimal?)ShowLookUpGrid(field, filter);
        }

        private decimal? ShowFilterView(ValueDataField field, string filter)
        {
            var model = new DialogSourceViewModel()
            {
                PanelCaption = string.Format("Поиск по номеру/ \n описанию/расш.описанию \n артикула"),
                FontSize = _fontSize,
                IsMenuVisible = true
            };

            var filterField = new ValueDataField
            {
                Name = "Filter",
                Caption = string.Empty,
                FieldType = typeof(string),
                LabelPosition = "Left",
                IsEnabled = true,
                SetFocus = true,
                CloseDialog = true
            };

            var infoField = new ValueDataField
            {
                Name = "desc",
                FieldName = "desc",
                SourceName = "desc",
                FieldType = typeof(String),
                Order = 2,
                LabelPosition = "None",
                Visible = true,
                IsEnabled = false,
                Value =
                    "По введенному ШК ничего не найдено.\n Введите не менее 5 символов (номер/описание/расширенное описание артикула) для поиска."
            };

            model.Fields.Add(filterField);
            model.Fields.Add(infoField);

            model.UpdateSource();

            var viewService = IoC.Instance.Resolve<IViewService>();

            while (true)
            {
                if (viewService.ShowDialogWindow(model, false) == true)
                {
                    if (String.IsNullOrEmpty((string)model[filterField.Name]) ||
                        ((string)model[filterField.Name]).Length < 5)
                        continue;

                    var commonFilter =
                        (string)SerializationHelper.ConvertToTrueType(model[filterField.Name], filterField.FieldType);

                    filter =
                        String.Format(
                            "MANDANTID = {0} and (upper(art.ARTNAME) like upper('%{1}%') or upper(art.ARTDESC) like upper('%{1}%') or upper(art.ARTDESCEXT) like upper('%{1}%'))",
                            _mandantId, commonFilter);

                    var skuId = (decimal?)ShowLookUpGrid(field, filter);

                    if (skuId == null)
                        continue;
                    return skuId;
                }
                return null;
            }
        }

        private object ShowLookUpGrid(ValueDataField field, string filter)
        {
            var bestFitColumnNames = new List<string>();

            var model = new DialogSourceViewModel()
            {
                PanelCaption = string.Format("Выбор '{0}'", field.Caption),
                FontSize = _fontSize,
                IsMenuVisible = false
            };

            var gridField = new ValueDataField
            {
                Name = field.Name,
                Caption = string.Empty,
                FieldType = field.FieldType,
                LookupCode = field.LookupCode,
                LookupFilterExt = filter,
                LabelPosition = "Left",
                IsEnabled = true,
                SetFocus = true,
                CloseDialog = true
            };
            gridField.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
            gridField.Set(ValueDataFieldConstants.ShowControlMenu, false);
            gridField.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, true);
            gridField.Set(ValueDataFieldConstants.ShowAutoFilterRow, true);
            gridField.Set(ValueDataFieldConstants.DoNotActionOnEnterKey, false);


            // выставим свой порядок полей для SKU
            if (field.Name == SkuIdRPropertyName)
            {
                gridField.Set(ValueDataFieldConstants.Fields, GetSkuFields().ToArray());
                bestFitColumnNames.AddRange(GetSkuFields().ToArray().Select(i => i.FieldName));
                gridField.Set(ValueDataFieldConstants.BestFitColumnNames, bestFitColumnNames.ToArray());
            }

            model.Fields.Add(gridField);

            model.UpdateSource();

            var viewService = IoC.Instance.Resolve<IViewService>();
            if (viewService.ShowDialogWindow(model, false) == true)
                return SerializationHelper.ConvertToTrueType(model[gridField.Name], gridField.FieldType);
            return null;
        }

        private IEnumerable<DataField> GetSkuFields()
        {
            var result = new List<DataField>();
            var fieldList = DataFieldHelper.Instance.GetDataFields(typeof(SKU), SettingDisplay.LookUp);

            // SKUNAME
            var skuName = fieldList.FirstOrDefault(i => i.Name == SKU.SKUNamePropertyName);
            if (skuName != null)
                result.Add(skuName);
            // ARTDESC
            var artDesc = fieldList.FirstOrDefault(i => i.Name == SKU.VARTDESCPropertyName);
            if (artDesc != null)
                result.Add(artDesc);
            // VMEASURENAME
            var skuMeasure = fieldList.FirstOrDefault(i => i.Name == SKU.VMEASURENAMEPropertyName);
            if (skuMeasure != null)
                result.Add(skuMeasure);
            // VArtName
            var vArtName = fieldList.FirstOrDefault(i => i.Name == "VARTNAME");
            if (vArtName != null)
                result.Add(vArtName);
            // SKUCOUNT
            var skuCount = fieldList.FirstOrDefault(i => i.Name == SKU.SKUCountPropertyName);
            if (skuCount != null)
                result.Add(skuCount);
            // SKUDESC
            var skuDesc = fieldList.FirstOrDefault(i => i.Name == SKU.SKUDESCPropertyName);
            if (skuDesc != null)
                result.Add(skuDesc);
            // SKUPrimary
            var skuPrimary = fieldList.FirstOrDefault(i => i.Name == SKU.SKUPrimaryPropertyName);
            if (skuPrimary != null)
                result.Add(skuPrimary);
            // SKUClient
            var skuClient = fieldList.FirstOrDefault(i => i.Name == SKU.SKUClientPropertyName);
            if (skuClient != null)
                result.Add(skuClient);
            // skuIndivisible
            var skuIndivisible = fieldList.FirstOrDefault(i => i.Name == SKU.SKUINDIVISIBLEPropertyName);
            if (skuIndivisible != null)
                result.Add(skuIndivisible);
            // skuDefault
            var skuDefault = fieldList.FirstOrDefault(i => i.Name == SKU.SKUDefaultPropertyName);
            if (skuDefault != null)
                result.Add(skuDefault);

            // остальные поля
            result.AddRange(fieldList.Except(result));
            return result;
        }

        private IEnumerable<ValueDataField> GetDefaultFields(IEnumerable<ValueDataField> fields, bool byPlace = false, bool surplus = false)
        {
            var order = 0;
            var result = new List<ValueDataField>();

            // Место
            var placeCodeField = fields.FirstOrDefault(i => i.Name == PlaceCodePropertyName);
            if (placeCodeField == null)
            {
                placeCodeField = new ValueDataField()
                {
                    Name = PlaceCodeRPropertyName,
                    Caption = "Место",
                    FieldName = PlaceCodeRPropertyName,
                    SourceName = PlaceCodePropertyName,
                    FieldType = typeof(string),
                    IsEnabled = false,
                    LookupCode = "PLACE_PLACECODE",
                    LabelPosition = "Left",
                    IsRequired = true
                };
            }
            placeCodeField.Order = order++;
            result.Add(placeCodeField);

            // ШК SKU 
            result.Add(new ValueDataField()
            {
                Name = BarCodePropertyName,
                Caption = "ШК",
                FieldName = BarCodePropertyName,
                SourceName = BarCodePropertyName,
                FieldType = typeof(string),
                IsEnabled = true,
                LabelPosition = "Left",
                IsRequired = false,
                Order = order++
            });

            // SKU
            var skuField = fields.FirstOrDefault(i => i.Name == SkuIdPropertyName);
            if (skuField == null)
            {
                skuField = new ValueDataField()
                {
                    Name = SkuIdRPropertyName,
                    Caption = "ЕУ",
                    FieldName = SkuIdRPropertyName,
                    SourceName = SkuIdPropertyName,
                    FieldType = typeof(decimal?),
                    IsEnabled = false,
                    LookupCode = "SKU_SKUID",
                    LookupFilterExt = string.Format("mandantid = {0}", _mandantId),
                    LabelPosition = "Left",
                    IsRequired = true
                };
            }
            skuField.Order = order++;
            result.Add(skuField);

            var artDescField = new ValueDataField()
            {
                Name = ArtDescPropertyName,
                Caption = "Опис.",
                FieldName = ArtDescPropertyName,
                SourceName = ArtDescPropertyName,
                FieldType = typeof(string),
                IsEnabled = false,
                LabelPosition = "None",
                IsRequired = false,
                Order = order++
            };
            result.Add(artDescField);

            // ТЕ
            var teCodeField = fields.FirstOrDefault(i => i.Name == TeCodePropertyName);
            if (teCodeField == null)
            {
                teCodeField = new ValueDataField()
                {
                    Name = TeCodePropertyName,
                    Caption = "ТЕ",
                    FieldName = TeCodePropertyName,
                    SourceName = TeCodePropertyName,
                    FieldType = typeof(string),
                    IsEnabled = true,
                    LabelPosition = "Left",
                    IsRequired = true
                };
            }
            teCodeField.Order = order++;
            result.Add(teCodeField);

            // Кол-во в SKU
            var skuCountField = fields.FirstOrDefault(i => i.Name == Count2SkuPropertyName);
            if (skuCountField == null)
            {
                skuCountField = new ValueDataField()
                {
                    Name = Count2SkuPropertyName,
                    Caption = "Кол-во (ЕУ)",
                    FieldName = Count2SkuPropertyName,
                    SourceName = Count2SkuPropertyName,
                    FieldType = typeof(double?),
                    IsEnabled = true,
                    LabelPosition = "Left",
                    IsRequired = true
                };
            }
            skuCountField.Order = order++;
            result.Add(skuCountField);

            // Тип ТЕ
            var teTypeField = fields.FirstOrDefault(i => i.Name == TeTypeCodePropertyName);
            if (teTypeField == null)
            {
                teTypeField = new ValueDataField()
                {
                    Name = TeTypeCodeRPropertyName,
                    Caption = "Тип ТЕ",
                    FieldName = TeTypeCodeRPropertyName,
                    SourceName = TeTypeCodePropertyName,
                    FieldType = typeof(string),
                    IsEnabled = false,
                    LookupCode = "TETYPE_TETYPECODE",
                    LabelPosition = "Left"
                };
            }
            // мы сами управляем типом ТЕ
            teTypeField.IsRequired = false;
            teTypeField.Order = order++;
            result.Add(teTypeField);

            // получим остальные поля и проставим порядок
            var otherFields = fields.Where(i => !result.Exists(j => j.Name == i.Name));
            foreach (var f in otherFields)
            {
                f.Order = order++;
            }
            result.AddRange(otherFields);

            // Кол-во
            var amountField = new ValueDataField()
            {
                Name = CountPropertyName,
                Caption = "Кол-во",
                FieldName = CountPropertyName,
                SourceName = CountPropertyName,
                FieldType = typeof(decimal?),
                IsEnabled = true,
                LabelPosition = "Left",
                SetFocus = true,
                IsRequired = true,
                Value = byPlace && !surplus ? (decimal?)1 : new decimal?(),
                Order = order++
            };

            result.Add(amountField);

            foreach (var field in result.Where(f => !string.IsNullOrEmpty(f.Caption)))
            {
                if (field.Get<string>(ValueDataFieldConstants.LayoutGroupName) == null &&
                    field.Caption.Length > MinCaptionLength)
                    field.Set(ValueDataFieldConstants.LayoutGroupName,
                        string.Format("Group_{0}_{1}", field.Name, field.Order));
                if (field.Caption.Length >= MaxCaptionLength)
                    field.LabelPosition = "Top";
            }
            amountField.Set(ValueDataFieldConstants.LayoutGroupName, "AmountLayoutGroup");
            skuCountField.Set(ValueDataFieldConstants.LayoutGroupName, "AmountLayoutGroup");

            return result;
        }

        private bool? ShowViewModel(IViewModel model)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            return viewService.ShowDialogWindow(model, false);
        }

        private void ShowMessage(string title, string message, bool error = false)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            viewService.ShowDialog(title, message, MessageBoxButton.OK,
                error ? MessageBoxImage.Warning : MessageBoxImage.Asterisk,
                MessageBoxResult.OK, _fontSize);
        }

        #endregion
    }
}