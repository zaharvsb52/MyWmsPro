using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.Activities.ViewInteraction
{
    public class ShowCustomObjectListViewActivity<T> : NativeActivity<T[]>
    {
        #region . InOutArguments .

        /// <summary>
        /// Результат диалога
        /// </summary>
        [DisplayName(@"Результат диалога")]
        public OutArgument<bool> DialogResult { get; set; }

        /// <summary>
        /// Данные извне
        /// </summary>
        [DisplayName(@"Данные")]
        [RequiredArgument, DefaultValue(null)]
        public InArgument<List<T>> Source { get; set; }

        #endregion

        #region . Properties .

        /// <summary>
        /// Заголовок
        /// </summary>
        [DisplayName(@"Заголовок")]
        public string Title { get; set; }

        [DisplayName(@"Ширина диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogWidth { get; set; }
        [DisplayName(@"Высота диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogHeight { get; set; }

        [DisplayName(@"Модель детализации")]        
        public InArgument<IObjectViewModel<T>> ObjectViewModel { get; set; }
        
        #endregion

        public ShowCustomObjectListViewActivity()
        {
            DisplayName = "Пользовательский список объектов";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var width = DialogWidth.Get(context);
            var height = DialogHeight.Get(context);
            var modelType = typeof(CustomObjectListViewModelBase<>).MakeGenericType(typeof(T));
            var model = (ICustomListViewModel<T>)IoC.Instance.Resolve(modelType);
            var detail = ObjectViewModel.Get(context);
            model.PanelCaption = Title;
            model.ObjectViewModel = detail;
            var source = Source.Get(context);
            model.SetSource(new ObservableCollection<T>(source));
            ((ICustomDisposable)model).SuspendDispose = true;
            var viewService = IoC.Instance.Resolve<IViewService>();
            if (viewService.ShowDialogWindow((IViewModel)model, true, false, width, height) == true)
            {
                var result = model.GetSource() as IEnumerable<T>;
                if (result == null)
                    throw new DeveloperException("Source type is not IEnumarable");
                Result.Set(context, result.ToArray());
                ((ICustomDisposable)model).SuspendDispose = false;
                ((IDisposable) model).Dispose();
                DialogResult.Set(context, true);
            }
            else
            {
                DialogResult.Set(context, false);
            }
            var disposable = model as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}
