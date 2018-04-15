using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.Activities.ViewInteraction
{
    public class ShowCustomObjectViewModelActivity<T> : NativeActivity
    {
        public ShowCustomObjectViewModelActivity()
        {
            DisplayName = @"Показать модель";
            Buttons = MessageBoxButton.OKCancel;
        }

        [DisplayName(@"Заголовок")]
        public InArgument<string> Title { get; set; }

        [RequiredArgument]
        [DisplayName(@"Модель")]
        public InOutArgument<T> Model { get; set; }

        [DisplayName(@"Результат диалога")]
        public OutArgument<bool> DialogResult { get; set; }

        [DisplayName(@"Ширина диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogWidth { get; set; }

        [DisplayName(@"Высота диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogHeight { get; set; }

        [DisplayName(@"Настройка (Layout)")]
        public InArgument<string> Layout { get; set; }

        [DisplayName(@"Добавлять элементы из Available Items")]
        [DefaultValue(false)]
        public InArgument<bool> InsertFromAvailableItems { get; set; }

        [DisplayName(@"Не показывать Ok, Cancel")]
        [DefaultValue(false)]
        public bool NoButtons { get; set; }

        [DisplayName(@"Не загружать настройки при открытии")]
        [Description("Если задан данный флаг, то при открытии формы не будет производиться загрузка файла настроек")]
        public bool DoNotLoadSettings { get; set; }

        [DisplayName(@"Кнопки диалога")]
        [DefaultValue(MessageBoxButton.OKCancel)]
        public MessageBoxButton Buttons { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Title, type.ExtractPropertyName(() => Title));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Model, type.ExtractPropertyName(() => Model));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResult, type.ExtractPropertyName(() => DialogResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogWidth, type.ExtractPropertyName(() => DialogWidth));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogHeight, type.ExtractPropertyName(() => DialogHeight));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Layout, type.ExtractPropertyName(() => Layout));
            ActivityHelpers.AddCacheMetadata(collection, metadata, InsertFromAvailableItems, type.ExtractPropertyName(() => InsertFromAvailableItems));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var width = DialogWidth.Get(context);
            var height = DialogHeight.Get(context);
            var model = Model.Get(context);
            var panelViewModelBase = model as PanelViewModelBase;
            if(panelViewModelBase != null)
                panelViewModelBase.PanelCaption = Title.Get(context);

            var customModelHandler = model as ICustomModelHandler;
            if (customModelHandler != null)
            {
                if (Layout != null)
                {
                    customModelHandler.LayoutValue = Layout.Get(context);
                    if (InsertFromAvailableItems != null)
                        customModelHandler.InsertFromAvailableItems = InsertFromAvailableItems.Get(context);
                }

                customModelHandler.CreateCustomMenu();
            }

            var viewService = IoC.Instance.Resolve<IViewService>();
            var iObj = model as IViewModel;
            if (iObj == null)
                throw new DeveloperException("Model is not IViewModel.");

            var required = true;
            bool? dialogresult = true;

            while (required && dialogresult == true)
            {
                dialogresult = viewService.ShowDialogWindow(iObj, !DoNotLoadSettings, true, width, height, NoButtons, Buttons);
                if (dialogresult != true) 
                    break;
                var exModel = model as ExpandoObjectViewModelBase;
                if (exModel == null) 
                    break;
                required = exModel.Fields.Any(f => f.IsRequired && (exModel[f.Name] == null || string.IsNullOrEmpty(exModel[f.Name].ToString())));
                if (!required) 
                    break;
                var requiredFields = string.Join(", ", exModel.Fields.Where(f => f.IsRequired && (exModel[f.Name] == null || string.IsNullOrEmpty(exModel[f.Name].ToString()))).Select(x => x.Caption));
                dialogresult = Equals(viewService.ShowDialog("Ошибка",
                    string.Format("Не заполнены обязательные поля: {0}\r\nПовторить ввод?", requiredFields),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error,
                    MessageBoxResult.Yes), MessageBoxResult.Yes);
            }

            Model.Set(context, model);
            DialogResult.Set(context, dialogresult == true);
        }
    }

    public abstract class BaseShowActivity : NativeActivity
    {
        #region .  Properties  .
        [DisplayName(@"Вариант открытия")]
        public DockType DockType { get; set; }

        [DisplayName(@"Открывать в новом окне")]
        [Description("Если установлен данный флаг, то открытие окна будет происходить без поиска уже имеющегося")]
        public bool ShowInNewWindow { get; set; }

        [DisplayName(@"Загружать настройки")]
        [Description("Будут загружены настройки отображения")]
        public bool LoadLayoutSettings { get; set; } 
        #endregion

        protected virtual ShowContext GetShowContext(NativeActivityContext context)
        {
            return new ShowContext
            {
                DockingType = DockType,
                ShowInNewWindow = ShowInNewWindow,
                LoadLayoutSettings = LoadLayoutSettings
            };
        }
    }

    public class ShowByCommandActivity : BaseShowActivity
    {
        public ShowByCommandActivity()
        {
            DisplayName = @"Открыть панель по команде";
        }

        #region .  Properties  .

        [RequiredArgument]
        [DisplayName(@"Команда")]
        public InOutArgument<string> Command { get; set; }

        #endregion

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Command, type.ExtractPropertyName(() => Command));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var vs = IoC.Instance.Resolve<IViewService>();

            var showContext = GetShowContext(context);

            var command = Command.Get(context);

            vs.Show(command, showContext);
        }
    }

    public class ShowByViewModelActivity : BaseShowActivity
    {
        public ShowByViewModelActivity()
        {
            DisplayName = @"Открыть панель по модели";
        }

        #region .  Properties  .

        [RequiredArgument]
        [DisplayName(@"Модель")]
        public InOutArgument<IViewModel> Model { get; set; }

        #endregion

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Model, type.ExtractPropertyName(() => Model));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var vs = IoC.Instance.Resolve<IViewService>();

            var showContext = GetShowContext(context);

            var viewModel = Model.Get(context);

            //var interfaceModel = viewModel as IViewModel;
            //if (interfaceModel == null)
            //    throw new DeveloperException("Object model is not IViewModel");

            vs.Show(viewModel, showContext);
        }
    }

}
