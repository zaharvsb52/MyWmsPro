using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.Activities.General.Helpers;
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
    public class GetPlLstActivity : NativeActivity
    {
        #region .  Arguments  .

        /// <summary>
        /// Коллекция активных списков
        /// </summary>
        [DisplayName(@"Коллекция активных списков")]
        [Description("Входная коллекция активных списков подбора")]
        public InArgument<List<MutableTuple<Int32,PL,TE>>> ActPlLst { get; set; }

        /// <summary>
        /// Вид списка полей для списка товара
        /// </summary>
        [DisplayName(@"Вид списка полей для списка товара")]
        public SettingDisplay DisplaySetting { get; set; }

        /// <summary>
        /// Коллекция активных списков
        /// </summary>
        [DisplayName(@"Коллекция завершенных списков")]
        [Description("Входная коллекция завершенных списков подбора")]
        public InArgument<List<MutableTuple<Int32, PL, TE>>> CompletePlLst { get; set; }

        [DisplayName(@"Результат диалога")]
        public OutArgument<bool> DialogResult { get; set; }

        [DisplayName(@"Выбранный список")]
        [Description("Список пикинга, выбранный пользователем")]
        public OutArgument<MutableTuple<Int32, PL, TE>> SelectedPl { get; set; }

        #endregion

        #region .  Fields&Properties  .
        
        private DialogSourceViewModel _mainModel;
        private const string PlListPropertyName = "PlLst";
        private readonly List<string> _objNameGrid = new List<string>() { "Item1", "PLID", "VOWBID", "TECODE" };
        
        #endregion

        #region .  Methods  .

        public GetPlLstActivity()
        {
            DisplayName = "ТСД: Получение списков подбора";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, ActPlLst,
                type.ExtractPropertyName(() => ActPlLst));
            ActivityHelpers.AddCacheMetadata(collection, metadata, CompletePlLst,
                type.ExtractPropertyName(() => CompletePlLst));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResult,
                type.ExtractPropertyName(() => DialogResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, SelectedPl,
                type.ExtractPropertyName(() => SelectedPl));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var actPlList = ActPlLst.Get(context);
            var completePlList = CompletePlLst.Get(context);

            try
            {
                var dialogRet = ShowMain(completePlList != null && actPlList != null ? actPlList.Union(completePlList).ToList() : actPlList, context);

                DialogResult.Set(context, dialogRet);
            }
            catch (Exception ex)
            {
                var message = "Ошибка при получении списков пикинга. " +
                              ExceptionHelper.GetErrorMessage(ex, false);
                ShowWarning(message);
            }
        }

        private DialogSourceViewModel GetMainModel(List<MutableTuple<Int32, PL, TE>> activePlList)
        {
            if (_mainModel == null)
            {
                _mainModel = new DialogSourceViewModel
                {
                    PanelCaption = "Списки пикинга",
                    IsMenuVisible = false,
                    FontSize = 15
                };

                var plLst = new ValueDataField
                {
                    Name = PlListPropertyName,
                    Caption = string.Empty,
                    FieldType = typeof(MutableTuple<Int32, PL, TE>),
                    LabelPosition = "Left",
                    IsEnabled = true,
                    SetFocus = true,
                    CloseDialog = true
                };

                plLst.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
                plLst.Set(ValueDataFieldConstants.ShowControlMenu, true);
                plLst.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, false);
                plLst.Set(ValueDataFieldConstants.ShowAutoFilterRow, false);
                plLst.Set(ValueDataFieldConstants.DoNotActionOnEnterKey, false);
                plLst.Set(ValueDataFieldConstants.ItemsSource, activePlList);
                plLst.Set(ValueDataFieldConstants.CloseDialogOnSelectedItemChanged, true);

                var grdFields = GetGridFields(plLst.FieldType, DisplaySetting);

                plLst.Set(ValueDataFieldConstants.Fields, grdFields.ToArray());

                _mainModel.Fields.Add(plLst);
            }
            else
            {
                _mainModel.GetField(PlListPropertyName).Set(ValueDataFieldConstants.ItemsSource, activePlList);
            }

            _mainModel.UpdateSource();

            return _mainModel;
        }

        private bool ShowMain(List<MutableTuple<Int32, PL, TE>> plList,NativeActivityContext context)
        {
            var model = GetMainModel(plList);
            string menuResult;

            if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                return false;

            while (true)
            {
                switch (model.MenuResult)
                {
                    case "Value":
                        var id = model[PlListPropertyName] as MutableTuple<Int32, PL, TE>;
                        if (id == null)
                        {
                            ShowWarning("Список не выбран");
                            return false;
                        }
                        SelectedPl.Set(context,id);
                        model.UpdateSource();
                        break;
                    default:
                        return false;
                }
                return true;
            }
        }

        private static void ShowWarning(string message, string caption = "Подбор")
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

            foreach (var nameGr in _objNameGrid)
            {
                var capt = String.Empty;
                switch (nameGr)
                {
                    case "Item1":
                        capt = "№ списка";
                        break;
                    case "PLID":
                        capt = "ID списка";
                        break;
                    case "VOWBID":
                        capt = "Накладная";
                        break; 
                    case "TECODE":
                        capt = "ТЕ";
                        break;
                }

                var index = fieldList.IndexOf(fieldList.FirstOrDefault(x => x.Name == nameGr));
                if (index >= 0)
                fieldList[index].Caption = capt;
            }

            return
                _objNameGrid.Distinct()
                    .Select(propertyName => fieldList.FirstOrDefault(p => p.Name.EqIgnoreCase(propertyName)))
                    .Where(field => field != null)
                    .ToList();
        }

        #endregion
    }
}