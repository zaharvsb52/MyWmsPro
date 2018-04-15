using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    public class InputPlPosListViewActivity : NativeActivity<List<InputPlPos>>
    {
        #region . InOutArguments .

        /// <summary>
        /// Результат диалога.
        /// </summary>
        [DisplayName(@"Результат диалога")]
        public OutArgument<bool?> DialogResult { get; set; }

        /// <summary>
        /// Позиции листа пикинга.
        /// </summary>
        [DisplayName(@"Позиции листа пикинга")]
        [RequiredArgument, DefaultValue(null)]
        public InArgument<List<InputPlPos>> Source { get; set; }

        ///<summary>
        /// Информация о работе.
        /// </summary>
        [DisplayName(@"Выполнение работ")]
        [RequiredArgument, DefaultValue(null)]
        public OutArgument<List<Working>> WorkingProp { get; set; }

        [DisplayName(@"Код Workflow")]
        [Description("Код Workflow, запускающийся при нажатии на кнопку Ок.")]
        public InArgument<string> ActionWorkflowCode { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        [DisplayName(@"Заголовок")]
        public InArgument<string> Title { get; set; }

        [DisplayName(@"Ширина диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogWidth { get; set; }

        [DisplayName(@"Высота диалога")]
        [DefaultValue(null)]
        public InArgument<string> DialogHeight { get; set; }

        #endregion . InOutArguments .

        public InputPlPosListViewActivity()
        {
            DisplayName = "Форма виртуальных позиций списка пикинга";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResult, type.ExtractPropertyName(() => DialogResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogWidth, type.ExtractPropertyName(() => DialogWidth));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogHeight, type.ExtractPropertyName(() => DialogHeight));
            ActivityHelpers.AddCacheMetadata(collection, metadata, WorkingProp, type.ExtractPropertyName(() => WorkingProp));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ActionWorkflowCode, type.ExtractPropertyName(() => ActionWorkflowCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Title, type.ExtractPropertyName(() => Title));
            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var width = DialogWidth.Get(context);
            var height = DialogHeight.Get(context);
            var workings = WorkingProp.Get(context);
            if (workings == null)
                workings = new List<Working>();

            var vm = (InputPlPosListViewModel) IoC.Instance.Resolve(typeof (InputPlPosListViewModel));
            vm.PanelCaption = Title.Get(context);
            vm.ActionWorkflowCode = ActionWorkflowCode.Get(context);

            var source = Source.Get(context);
            if (source != null)
                source.ForEach(p => p.AcceptChanges());
            ((IModelHandler) vm).SetSource(new ObservableCollection<InputPlPos>(source));

            var viewService = IoC.Instance.Resolve<IViewService>();
            var dialogResult = viewService.ShowDialogWindow(viewModel: vm, isRestoredLayout: true, isNotNeededClosingOnOkResult: true, width: width, height: height);
            if (dialogResult == true)
            {
                var result = ((IModelHandler)vm).GetSource() as IEnumerable<InputPlPos>;
                if (result == null)
                    throw new DeveloperException("Source type is not IEnumerable.");

                Result.Set(context, result.ToList());
            }
            DialogResult.Set(context, dialogResult);

            workings.Clear();
            if (vm.Workings != null)
            {
                foreach (var w in vm.Workings)
                {
                    var working = new Working();

                    try
                    {
                        working.SuspendNotifications();
                        WMSBusinessObject.Copy(w, working);
                        working.AcceptChanges();
                    }
                    finally
                    {
                        working.ResumeNotifications();
                    }
                    workings.Add(working);
                }
            }
            WorkingProp.Set(context, workings);

            var disposable = vm as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

    }
}