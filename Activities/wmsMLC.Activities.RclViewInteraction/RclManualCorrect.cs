using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclManualCorrect<T> : NativeActivity
    {
        #region .  Properties  .
        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Объект корректировки")]
        public InOutArgument<T> Entity { get; set; }

        [DisplayName(@"Заголовок")]
        public InArgument<string> PanelCaption { get; set; }

        [DisplayName(@"Код операции")]
        public InArgument<string> OperationCode { get; set; }

        [DisplayName(@"Номер SKU")]
        public InArgument<Decimal> SKU { get; set; }

        [DisplayName(@"Максимальное количество строк в Lookup'е")]
        public InArgument<int> MaxRowsOnPage { get; set; }

        #endregion

        public RclManualCorrect()
        {
            DisplayName = "ТСД: Корректировака параметров объекта";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Entity, type.ExtractPropertyName(() => Entity));
            ActivityHelpers.AddCacheMetadata(collection, metadata, PanelCaption, type.ExtractPropertyName(() => PanelCaption));
            ActivityHelpers.AddCacheMetadata(collection, metadata, SKU, type.ExtractPropertyName(() => SKU));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode, type.ExtractPropertyName(() => OperationCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MaxRowsOnPage, type.ExtractPropertyName(() => MaxRowsOnPage));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var entity = Entity.Get(context);
            var obj = entity as WMSBusinessObject;
            if (obj == null)
                throw new NotImplementedException(string.Format("Тип '{0}' не поддерживается", entity.GetType()));

            var pos = 0;
            // получим параметры MUST_SET
            var manualCorrectList = GetManualCorrectProperies(context).ToArray();
            var objFields = DataFieldHelper.Instance.GetDataFields(typeof(T), wmsMLC.General.PL.SettingDisplay.Detail);
            for (; pos < manualCorrectList.Length; )
            {
                var m = manualCorrectList[pos];
                var datafield = objFields.FirstOrDefault(i => i.Name.EqIgnoreCase(m.ObjectName_r));
                if (datafield == null)
                    throw new DeveloperException("Ошибка в настройках CAN_PRODUCT_MANUAL_CORRECT менеджера товара. Задан неизвестный параметр '{0}'.", m.ObjectName_r);

                var vfield = new ValueDataField(datafield)
                {
                    LabelPosition = "Top",
                    Value = obj.GetProperty(datafield.SourceName)
                };

                //Если поле лукап
                if (!string.IsNullOrEmpty(vfield.LookupCode))
                {
                    vfield.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectControl.ToString());
                    vfield.Set(ValueDataFieldConstants.MaxRowsOnPage, MaxRowsOnPage.Get(context));
                }

                var model = GetMainModel(context, vfield, pos != 0);
                while (true)
                {
                    if (ShowDialog(model) == false)
                        return;

                    var value = model[vfield.Name];
                    obj.SetProperty(vfield.SourceName, value);
                    switch (model.MenuResult)
                    {
                        case "F1":
                            pos++;
                            break;
                        case "F2":
                            pos--;
                            break;
                    }
                    break;
                }
            }
            Entity.Set(context, obj);
        }

        private DialogSourceViewModel GetMainModel(NativeActivityContext context, ValueDataField property, bool canMovePrev = true)
        {
            var model = new DialogSourceViewModel
            {
                PanelCaption = PanelCaption.Get(context),
                FontSize = FontSize.Get(context),
                IsMenuVisible = false,
            };

            // добавим параметр
            property.SetFocus = true;
            model.Fields.Add(property);

            var footerMenuItem = new ValueDataField
            {
                Name = "footerMenu",
                FieldType = typeof(IFooterMenu),
                Visible = true,
                IsEnabled = true
            };
            footerMenuItem.FieldName = footerMenuItem.Name;
            footerMenuItem.SourceName = footerMenuItem.Name;
            model.Fields.Add(footerMenuItem);

            var footerMenu = new List<ValueDataField>();
            if (canMovePrev)
            {
                var prevBtn = new ValueDataField {Name = "F2", Caption = "Назад", FieldType = typeof (Button)};
                prevBtn.FieldName = prevBtn.Name;
                prevBtn.SourceName = prevBtn.Name;
                prevBtn.Value = prevBtn.Name;
                footerMenu.Add(prevBtn);
            }

            var nextBtn = new ValueDataField {Name = "F1", Caption = "Далее", FieldType = typeof (Button)};
            nextBtn.FieldName = nextBtn.Name;
            nextBtn.SourceName = nextBtn.Name;
            nextBtn.Value = nextBtn.Name;
            footerMenu.Add(nextBtn);

            footerMenuItem.Set(ValueDataFieldConstants.FooterMenu, footerMenu.ToArray());

            model.UpdateSource();
            return model;
        }

        private bool ShowDialog(DialogSourceViewModel model)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            if (viewService.ShowDialogWindow(model, false) != true)
                return false;
            return true;
        }

        private IEnumerable<PMConfig> GetManualCorrectProperies(NativeActivityContext context)
        {
            SKU sku;
            var skuId = SKU.Get(context);
            var operationCode = OperationCode.Get(context);
            using (var mgr = GetManager<SKU>(context))
                sku = mgr.Get(skuId);

            if (sku == null)
                throw new DeveloperException(string.Format("SKU с кодом '{0}' не существует", skuId));

            var art = sku.ArtCode;
            using (var pmConfigMgr = IoC.Instance.Resolve<IPMConfigManager>())
            {
                SetUnitOfWork(context, pmConfigMgr);
                return pmConfigMgr.GetPMConfigByParamListByArtCode(art, operationCode, "CAN_PRODUCT_MANUAL_CORRECT");
            }
        }

        private IBaseManager<TM> GetManager<TM>(NativeActivityContext context)
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<TM>>();
            SetUnitOfWork(context, mgr);
            return mgr;
        }

        private void SetUnitOfWork(NativeActivityContext context, IBaseManager mgr)
        {
            var uw = BeginTransactionActivity.GetUnitOfWork(context);
            if (uw != null)
                mgr.SetUnitOfWork(uw);
        }
    }
}
