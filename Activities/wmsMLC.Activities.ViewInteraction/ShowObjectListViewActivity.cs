using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    public class ShowObjectListViewActivity<T> : NativeActivity<T[]>
    {
        #region . InOutArguments .
        /// <summary>
        /// Фильтр для выборки
        /// </summary>
        [DisplayName(@"Фильтр для выборки")]
        public InArgument<string> Filter { get; set; }
        /// <summary>
        /// Результат диалога
        /// </summary>
        [DisplayName(@"Результат диалога")]
        public OutArgument<MessageBoxResult> DialogResult { get; set; }

        /// <summary>
        /// Данные извне
        /// </summary>
        [DisplayName(@"Данные (игнорирует фильтр)")]
        [RequiredArgument, DefaultValue(null)]
        public InArgument<List<T>> Source { get; set; }
        #endregion

        #region . Properties .
        /// <summary>
        /// Заголовок
        /// </summary>
        [DisplayName(@"Заголовок")]
        public string Title { get; set; }
        
        /// <summary>
        /// Отображаемые кнопки
        /// </summary>
        [DisplayName(@"Кнопки диалога")]
        public MessageBoxButton Buttons { get; set; }

        /// <summary>
        /// Изображение
        /// </summary>
        [DisplayName(@"Иконка")]
        public MessageBoxImage Image { get; set; }

        /// <summary>
        /// Результат по умолчанию
        /// </summary>
        [DisplayName(@"Кнопка по умолчанию")]
        public MessageBoxResult DefaultResult { get; set; }

        /// <summary>
        /// Выбор первого эл-та из списка по умолчанию
        /// </summary>
        [DisplayName(@"Выбор первого из списка")]
        public bool IsSelectedFirstItem { get; set; }

        /// <summary>
        /// Закрытие окна по двойному клику по item
        /// </summary>
        [DisplayName(@"ОК по двойному клику")]
        public bool IsCloseDoubleClick { get; set; }

        [DisplayName(@"Показывать меню настройки вида")]
        [Description("Если данный флаг установлен, то пользователю будет выведено меню с возможностью сохранить вид")]
        public bool ShowCustomizeMenu { get; set; }

        [DisplayName(@"Суффикс файла настроек")]
        [Description("Уникальный суффикс, который будет добавлен к имени файла настроек при их чтении/записи")]
        public string LayoutSettingsFileSuffix { get; set; }

        [DisplayName(@"Ширина диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogWidth { get; set; }
        [DisplayName(@"Высота диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogHeight { get; set; }

        #endregion

        public ShowObjectListViewActivity()
        {
            DisplayName = "Выбрать записи сущности";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var width = DialogWidth.Get(context);
            var height = DialogHeight.Get(context);
            var modelType = typeof(ListViewModelBase<>).MakeGenericType(typeof(T));
            var model = (IListViewModel<T>)IoC.Instance.Resolve(modelType);
            model.PanelCaption = Title;
            var modelBase = (ListViewModelBase<T>) model;
            modelBase.IsMainMenuEnable = false;
            if (ShowCustomizeMenu)
            {
                if (!string.IsNullOrEmpty(LayoutSettingsFileSuffix))
                    modelBase.SetSuffix(LayoutSettingsFileSuffix);
                modelBase.IsCustomizeBarEnabled = true;
                if (modelBase.Menu != null)
                    modelBase.Menu.NotUseGlobalLayoutSettings = true;

                //foreach (var bar in modelBase.Menu.Bars)
                //{
                //    if (!(bar != null && !string.IsNullOrEmpty(bar.Caption) && bar.Caption.Equals("Вид")))
                //        modelBase.Menu.Bars.Remove(bar);
                //}
            }
            else
            {
                //modelBase.Menu.Bars.Clear();
                modelBase.IsCustomizeBarEnabled = false;
            }

            var source = Source.Get(context);
            if (source == null)
            {
                var sqlFilter = Filter.Get(context);
                model.ApplySqlFilter(sqlFilter);
            }
            else
            {
                //model.SetSource(new EditableBusinessObjectCollection<T>(source));
                model.SetSource(new ObservableRangeCollection<T>(source));
            }

            model.IsSelectedFirstItem = IsSelectedFirstItem;

            if (IsCloseDoubleClick)
                model.IsCloseDoubleClick = true;

            modelBase.SuspendDispose = true;

            var viewService = IoC.Instance.Resolve<IViewService>();
            if (viewService.ShowDialogWindow(model, true, false, width, height, false, Buttons) == true)
            {
                Result.Set(context, model.SelectedItems.ToArray());
                modelBase.SuspendDispose = false;
                modelBase.Dispose();                
                DialogResult.Set(context, MessageBoxResult.OK);
            }
            else
            {
                DialogResult.Set(context, MessageBoxResult.Cancel);
            }
        }
    }
}
