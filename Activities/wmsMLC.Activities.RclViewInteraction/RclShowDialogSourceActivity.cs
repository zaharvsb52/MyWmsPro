using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.Dialogs.Models;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.RclViewInteraction
{
    /// <summary>
    /// Диалог с меню для терминала.
    /// </summary>
    public class RclShowDialogSourceActivity : NativeActivity
    {
        public RclShowDialogSourceActivity()
        {
            DisplayName = "ТСД: Ввод данных пользователем";
        }
        
        [RequiredArgument]
        [DisplayName(@"Модель")]
        public InOutArgument<DialogModel> Model { get; set; }

        [DisplayName(@"Источник данных")]
        public InArgument<object> Source { get; set; }

        [DisplayName(@"Показать меню")]
        [DefaultValue(true)]
        public InArgument<bool> IsMenuVisible { get; set; }

        [DisplayName(@"Меню")]
        public InArgument<ValueDataField[]> MenuItems { get; set; }

        [DisplayName(@"Результат диалога")]
        public OutArgument<bool> DialogResult { get; set; }

        [DisplayName(@"Результат диалога с учетом меню")]
        public OutArgument<string> DialogResultWithMenu { get; set; }

        [DisplayName(@"Настройка (Layout)")]
        public InArgument<string> Layout { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Model, type.ExtractPropertyName(() => Model));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsMenuVisible, type.ExtractPropertyName(() => IsMenuVisible));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MenuItems, type.ExtractPropertyName(() => MenuItems));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResult, type.ExtractPropertyName(() => DialogResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResultWithMenu, type.ExtractPropertyName(() => DialogResultWithMenu));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Layout, type.ExtractPropertyName(() => Layout));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var model = Model.Get(context);
            var source = Source.Get(context);
            var dlgmodel = new DialogSourceViewModel
                {
                    PanelCaption = model.Header,
                    IsMenuVisible = IsMenuVisible.Get(context),
                    FontSize = model.FontSize
                };

            var menu = MenuItems.Get(context);
            if (menu != null && menu.Length > 0)
            {
                dlgmodel.MenuItems.AddRange(menu);
                dlgmodel.CreateMenu();
            }

            UpdateFields(source, model);
            dlgmodel.Fields.AddRange(model.Fields);
            dlgmodel.UpdateSource();
            dlgmodel.LayoutValue = Layout.Get(context);

            string menuResult;
            var dialogResult = ShowDialog(dlgmodel, out menuResult);
            if (dialogResult)
            {
                model.Fields.Clear();
                model.Fields.AddRange(dlgmodel.Fields);
                Model.Set(context, model);
            }

            DialogResult.Set(context, dialogResult);
            if (DialogResultWithMenu != null)
                DialogResultWithMenu.Set(context, menuResult);
        }

        public void UpdateFields(object source, DialogModel model)
        {
            if (source == null)
                return;

            var pdsc = TypeDescriptor.GetProperties(source);
            var fields = model.Fields.Select(i => (ValueDataField)i.Clone()).ToArray();
            foreach (var field in fields.Where(x => !string.IsNullOrEmpty(x.Name)))
            {
                var property = pdsc.Find(field.Name, true);
                if (property != null)
                {
                    field.Value = property.GetValue(source);
                }
            }
            model.Fields.Clear();
            model.Fields.AddRange(fields);
        }

        public static bool ShowDialog(DialogSourceViewModel model, out string menuResult)
        {
            menuResult = "0";
            var viewService = IoC.Instance.Resolve<IViewService>();
            if (viewService.ShowDialogWindow(model, false) != true)
                return false;

            if (model.Fields != null)
            {
                foreach (var field in model.Fields)
                {
                    if (!model.ContainsKey(field.Name))
                        continue;

                    var value = model[field.Name];
                    if (!Equals(field.Value, value))
                        field.Value = value;
                }
            }

            menuResult = string.Format("1{0}", string.IsNullOrEmpty(model.MenuResult) ? string.Empty : model.MenuResult);
            return true;
        }
    }
}
