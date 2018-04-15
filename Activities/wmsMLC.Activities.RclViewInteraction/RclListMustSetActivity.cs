using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    public class RclListMustSetActivity<T> : NativeActivity
    {
        #region .  Properties  .
        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Объект валидации")]
        public InOutArgument<T> Entity { get; set; }

        [DisplayName(@"Номера SKU")]
        public InArgument<List<Art>> ArtLst { get; set; }

        [DisplayName(@"Код операции")]
        public InArgument<string> OperationCode { get; set; }

        [DisplayName(@"Максимальное количество строк в Lookup'е")]
        public InArgument<int> MaxRowsOnPage { get; set; }

        [DisplayName(@"Показывать ключ в заголовке")]
        public InArgument<bool> ShowKey { get; set; }
        [DefaultValue(false)]
        #endregion

        public RclListMustSetActivity()
        {
            DisplayName = "ТСД: Обязательные значения для коллекции артикулов";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ArtLst, type.ExtractPropertyName(() => ArtLst));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode, type.ExtractPropertyName(() => OperationCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Entity, type.ExtractPropertyName(() => Entity));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ShowKey, type.ExtractPropertyName(() => ShowKey));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MaxRowsOnPage, type.ExtractPropertyName(() => MaxRowsOnPage));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var entity = Entity.Get(context);
            var obj = entity as WMSBusinessObject;
            if (obj == null)
                throw new NotImplementedException(string.Format("Тип '{0}' не поддерживается", entity.GetType()));
            var objname = SourceNameHelper.Instance.GetSourceName(obj.GetType());
            // получим параметры MUST_SET
            var mustSetList = GetProperties(context);
            var pmConfigs = mustSetList as IList<PMConfig> ?? mustSetList.ToList();
            var mustProperties = pmConfigs.Select(pm => pm.ObjectName_r).Distinct();
            var objFields = DataFieldHelper.Instance.GetDataFields(typeof(T), wmsMLC.General.PL.SettingDisplay.Detail);

            // проход по каждому параметру вперед и назад
            foreach (var attrName in mustProperties)
            {
                var methods = pmConfigs.Where(pm => pm.ObjectName_r == attrName).Select(pm => pm.MethodCode_r).Distinct().ToList();
                var setNull = methods.Exists(m => m == "SET_NULL");
                var mustSet = methods.Exists(m => m == "MUST_SET");
                var mustCorrect = methods.Exists(m => m == "MUST_MANUAL_CORRECT");
                if (!setNull && !mustSet && !mustCorrect) continue;

                var attrValue = obj.GetProperty(attrName);
                var defaultValue = obj.GetPropertyDefaultValue(attrName);
                var datafield = objFields.FirstOrDefault(i => i.Name.EqIgnoreCase(attrName));
                if (datafield == null)
                    throw new DeveloperException("Ошибка в настройках менеджера товара. Задан неизвестный параметр '{0}.{1}'.", objname, attrName);
                var field = new ValueDataField(datafield) { LabelPosition = "Top" };
                // если у параметра есть значение по-умолчанию и оно совпадает с реальным, то необходимо подтверждение ввода
                if (!mustCorrect && !setNull && attrValue != null && !Equals(attrValue, defaultValue)) continue;

                // параметры атрибута
                field.Value = setNull
                    ? null
                    : (attrValue == null && defaultValue != null) 
                        ? defaultValue
                        : attrValue;

                field.IsRequired = mustSet;

                if (string.IsNullOrEmpty(field.LookupCode))
                {
                    field.CloseDialog = true;
                }
                else //Если поле лукап
                {
                    field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectControl.ToString());
                    field.Set(ValueDataFieldConstants.MaxRowsOnPage, MaxRowsOnPage.Get(context));
                }

                // *-* //
                var model = GetMainModel(context, field);
                var showKey = ShowKey.Get(context);
                while (true) // делаем это для возврата на форму
                {
                    if (showKey)
                        model.PanelCaption = string.Format("Введите значение ('{0}')", obj.GetKey());

                    if (!ShowDialog(model))
                        throw new OperationException("Не были указаны обязательные параметры.");

                    var errorMessage = new StringBuilder();
                    switch (string.Format("1{0}", model.MenuResult))
                    {
                        case "1":
                        case "1F1":
                            var value = model[field.Name];
                            if (value == null && field.IsRequired) 
                                errorMessage.AppendLine(string.Format("Не заполнено обязательное поле '{0}'.", field.Caption));
                            else
                                obj.SetProperty(field.Name, value);
                            break;
                        default:
                            throw new DeveloperException("Неизвестная команда.");
                    }
                    // если были ошибки, то покажем что не так и вернемся на форму
                    if (errorMessage.Length > 0)
                    {
                        var viewService = IoC.Instance.Resolve<IViewService>();
                        viewService.ShowDialog("Ошибка", errorMessage.ToString(), System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error, System.Windows.MessageBoxResult.OK);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Entity.Set(context, obj);
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
                Caption = "Дальше",
                FieldType = typeof(Button),
            };
            nextBtn.FieldName = nextBtn.Name;
            nextBtn.SourceName = nextBtn.Name;
            nextBtn.Value = nextBtn.Name;

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

        private IEnumerable<PMConfig> GetProperties(NativeActivityContext context)
        {
            var artL = ArtLst.Get(context);
            var entity = Entity.Get(context);
            var obj = entity as WMSBusinessObject;
            if (obj == null)
                throw new NotImplementedException(string.Format("Тип '{0}' не поддерживается", entity.GetType()));
            var objname = SourceNameHelper.Instance.GetSourceName(obj.GetType());
            var operationCode = OperationCode.Get(context);
            var resLst = new List<PMConfig>();
            using (var pmConfigMgr = IoC.Instance.Resolve<IPMConfigManager>())
            {
                SetUnitOfWork(context, pmConfigMgr);
                // параметры для сверки по артикулу и операции, фильтр по имени сущности
                foreach(var art in artL)
                    resLst.AddRange(pmConfigMgr.GetPMConfigByParamListByArtCode(art.ArtCode, operationCode, null).Where(pm => pm.ObjectEntitycode_R == objname.ToUpper()));
            }
            return resLst;
        }

        //private IBaseManager<TM> GetManager<TM>(NativeActivityContext context)
        //{
        //    var mgr = IoC.Instance.Resolve<IBaseManager<TM>>();
        //    SetUnitOfWork(context, mgr);
        //    return mgr;
        //}

        private void SetUnitOfWork(NativeActivityContext context, IBaseManager mgr)
        {
            var uw = BeginTransactionActivity.GetUnitOfWork(context);
            if (uw != null)
                mgr.SetUnitOfWork(uw);
        }
    }
}
