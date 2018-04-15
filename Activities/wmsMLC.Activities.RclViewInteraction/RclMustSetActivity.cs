using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public class RclMustSetActivity<T> : NativeActivity
    {
        public RclMustSetActivity()
        {
            DisplayName = "ТСД: Обязательные значения по SKU";
            FontSize = 14;
            MaxRowsOnPage = CustomSelectControlBase.MaxRows;
        }

        #region .  Properties  .
        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Объект валидации")]
        public InOutArgument<T> Entity { get; set; }

        [DisplayName(@"Номер SKU")]
        public InArgument<decimal> SKU { get; set; }

        [DisplayName(@"Код операции")]
        public InArgument<string> OperationCode { get; set; }

        [DisplayName(@"Максимальное количество строк в Lookup'е")]
        public InArgument<int> MaxRowsOnPage { get; set; }

        [DisplayName(@"История значений свойств")]
        public InOutArgument<Dictionary<string, object>> HistoryPropertyValue { get; set; }
        #endregion .  Properties  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, SKU, type.ExtractPropertyName(() => SKU));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode, type.ExtractPropertyName(() => OperationCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Entity, type.ExtractPropertyName(() => Entity));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MaxRowsOnPage, type.ExtractPropertyName(() => MaxRowsOnPage));
            ActivityHelpers.AddCacheMetadata(collection, metadata, HistoryPropertyValue, type.ExtractPropertyName(() => HistoryPropertyValue));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var entity = Entity.Get(context);
            var obj = entity as WMSBusinessObject;
            if (obj == null)
                throw new NotImplementedException(string.Format("Тип '{0}' не поддерживается", entity.GetType()));
            // получим параметры MUST_SET
            var mustSetList = GetMustSetProperies(context).ToArray();

            var objFields = DataFieldHelper.Instance.GetDataFields(typeof(T), wmsMLC.General.PL.SettingDisplay.Detail);
            var history = HistoryPropertyValue.Get(context);

            // проход по каждому параметру вперед и назад
            foreach (var m in mustSetList)
            {
                // если у параметра есть значение, то оно может быть по умолчанию
                var defaultValue = obj.GetPropertyDefaultValue(m.PmConfig.ObjectName_r);
                if (obj.GetProperty(m.PmConfig.ObjectName_r) == null || defaultValue != null || m.Show)
                {
                    var datafield = objFields.FirstOrDefault(i => i.Name.EqIgnoreCase(m.PmConfig.ObjectName_r));
                    if (datafield == null)
                        throw new DeveloperException("Ошибка в настройках {1} менеджера товара. Задан неизвестный параметр '{0}'.", m.PmConfig.ObjectName_r, m.PmConfig.MethodCode_r);

                    var field = new ValueDataField(datafield) { LabelPosition = "Top" };

                    if (defaultValue != null)
                        field.Value = defaultValue;

                    if (m.Show)
                        field.Value = obj.GetProperty(m.PmConfig.ObjectName_r);

                    //Массовый ввод
                    if (m.Set && m.PmConfig.PMCONFIGINPUTMASS == true && history != null && history.ContainsKey(field.Name))
                        field.Value = history[field.Name];

                    if (string.IsNullOrEmpty(field.LookupCode))
                    {
                        field.CloseDialog = true;
                    }
                    else //Если поле лукап
                    {
                        field.LabelPosition = "None";
                        field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectControl.ToString());
                        field.Set(ValueDataFieldConstants.MaxRowsOnPage, MaxRowsOnPage.Get(context));
                    }

                    var model = GetMainModel(context, field);
                    if (m.Show)
                        model.PanelCaption = model.PanelCaption + " [Опционально]";

                    while (true) // делаем это для возврата на форму
                    {
                        var show = ShowDialog(model);
                        if (!show && m.Set)
                            throw new OperationException("Не были указаны обязательные параметры.");

                        if (!show && m.Show)
                            break;

                        var errorMessage = new StringBuilder();
                        switch (string.Format("1{0}", model.MenuResult))
                        {
                            case "1":
                            case "1F1":
                                var value = model[field.Name];
                                if (value == null && m.Set)
                                {
                                    errorMessage.AppendLine(string.Format("Не заполнено обязательное поле '{0}'.",
                                        field.Caption));
                                }
                                else
                                {
                                    //Дополнительные параметры PMConfig
                                    if (m.Set && !string.IsNullOrEmpty(m.PmConfig.PMCONFIGINPUTMASK) && value is string)
                                    {
                                        var expressions = Regex.Matches((string) value, m.PmConfig.PMCONFIGINPUTMASK,
                                            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                        if (expressions.Count > 0)
                                            errorMessage.AppendLine(string.Format("Поле '{0}' не прошло проверку по маске.", field.Caption));
                                    }

                                    if (errorMessage.Length == 0)
                                    {
                                        obj.SetProperty(field.Name, value);
                                        if (history != null)
                                            history[field.Name] = value;
                                    }
                                }
                                break;
                            default:
                                throw new DeveloperException("Неизвестная команда.");
                        }
                        // если были ошибки, то покажем что не так и вернемся на форму
                        if (errorMessage.Length > 0)
                        {
                            var viewService = IoC.Instance.Resolve<IViewService>();
                            viewService.ShowDialog("Ошибка", errorMessage.ToString(),
                                System.Windows.MessageBoxButton.OK,
                                System.Windows.MessageBoxImage.Error, System.Windows.MessageBoxResult.OK);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            Entity.Set(context, obj);
            if (history != null)
                HistoryPropertyValue.Set(context, history);
        }

        private DialogSourceViewModel GetMainModel(NativeActivityContext context, ValueDataField property)
        {
            var model = new DialogSourceViewModel
            {
                PanelCaption = "Введите значение",
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

            var nextBtn = new ValueDataField
            {
                Name = "F1",
                Caption = "Далее",
                FieldType = typeof(Button),
            };
            nextBtn.FieldName = nextBtn.Name;
            nextBtn.SourceName = nextBtn.Name;
            nextBtn.Value = nextBtn.Name;
            nextBtn.Set(ValueDataFieldConstants.Row, 0);
            nextBtn.Set(ValueDataFieldConstants.Column, 1);

            footerMenuItem.Set(ValueDataFieldConstants.FooterMenu, new[] { nextBtn });

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

        private IEnumerable<MustSetShowConfig> GetMustSetProperies(NativeActivityContext context)
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
                var set = pmConfigMgr.GetPMConfigByParamListByArtCode(art, operationCode, "MUST_SET");
                var res = set.Select(item => new MustSetShowConfig(item, true)).ToList();
                var show = pmConfigMgr.GetPMConfigByParamListByArtCode(art, operationCode, "MUST_SHOW");

                foreach (var item in show)
                {
                    var p = res.FirstOrDefault(i => i.PmConfig.PM2OperationCode_r == item.PM2OperationCode_r &&
                                                    i.PmConfig.ObjectName_r == item.ObjectName_r &&
                                                    i.PmConfig.ObjectEntitycode_R == item.ObjectEntitycode_R);
                    if (p == null)
                        res.Add(new MustSetShowConfig(item, false) {Show = true});
                    else
                        p.Show = true;
                }
                return res;
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

        private class MustSetShowConfig
        {
            public MustSetShowConfig(PMConfig pmConfig, bool set)
            {
                PmConfig = pmConfig;
                Set = set;
            }

            public PMConfig PmConfig { get; }
            public bool Set { get; }
            public bool Show { get; set; }
        }
    }
}
