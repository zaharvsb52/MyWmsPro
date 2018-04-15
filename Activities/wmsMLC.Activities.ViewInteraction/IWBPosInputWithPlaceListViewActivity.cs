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
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    public class IWBPosInputWithPlaceListViewActivity : NativeActivity<IWBPosInput[]>
    {
        private const string PrintTEOnInput = "PrintTEOnInput";

        #region . InOutArguments .

        /// <summary>
        /// Результат диалога
        /// </summary>
        [DisplayName(@"Результат диалога")]
        public OutArgument<bool> DialogResult { get; set; }

        /// <summary>
        /// Печатать этикетки для ТЕ
        /// </summary>
        [DisplayName(@"Печатать этикетки для ТЕ")]
        public OutArgument<bool> PrintTE { get; set; }

        /// <summary>
        /// Место приемки
        /// </summary>
        [DisplayName(@"Место приемки")]
        public InOutArgument<Place> InvoicePlace { get; set; }

        /// <summary>
        /// Код манданта
        /// </summary>
        [DisplayName(@"Код манданта")]
        public InArgument<decimal?> MandantID { get; set; }

        /// <summary>
        /// Фильтр по местам
        /// </summary>
        [DisplayName(@"Фильтр места")]
        public InArgument<string> PlaceFilter { get; set; }

        /// <summary>
        /// Код операции
        /// </summary>
        [DisplayName(@"Код операции")]
        public InArgument<string> OperationCode { get; set; }

        /// <summary>
        /// Данные извне
        /// </summary>
        [DisplayName(@"Данные")]
        [RequiredArgument, DefaultValue(null)]
        public InArgument<List<IWBPosInput>> Source { get; set; }

        [DisplayName(@"Признак, что накладная создана миграцией")]
        [DefaultValue(false)]
        public InArgument<bool> IsMigration { get; set; }

        #endregion . InOutArguments .

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

        [DisplayName(@"Код workflow для расшифровки batch-кодов")]
        [DefaultValue(null)]
        public InArgument<string> BatchcodeWorkflowCode { get; set; }

        [DisplayName(@"Код workflow для изменения ОВХ SKU")]
        [DefaultValue(null)]
        public InArgument<string> SkuChangeMpWorkflowCode { get; set; }

        #endregion . Properties .

        public IWBPosInputWithPlaceListViewActivity()
        {
            DisplayName = "Форма позиций приходной накладной с местами";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogResult, type.ExtractPropertyName(() => DialogResult));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogWidth, type.ExtractPropertyName(() => DialogWidth));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogHeight, type.ExtractPropertyName(() => DialogHeight));
            ActivityHelpers.AddCacheMetadata(collection, metadata, PrintTE, type.ExtractPropertyName(() => PrintTE));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MandantID, type.ExtractPropertyName(() => MandantID));
            ActivityHelpers.AddCacheMetadata(collection, metadata, PlaceFilter, type.ExtractPropertyName(() => PlaceFilter));
            ActivityHelpers.AddCacheMetadata(collection, metadata, InvoicePlace, type.ExtractPropertyName(() => InvoicePlace));
            ActivityHelpers.AddCacheMetadata(collection, metadata, BatchcodeWorkflowCode, type.ExtractPropertyName(() => BatchcodeWorkflowCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, SkuChangeMpWorkflowCode, type.ExtractPropertyName(() => SkuChangeMpWorkflowCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsMigration, type.ExtractPropertyName(() => IsMigration));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode, type.ExtractPropertyName(() => OperationCode));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var width = DialogWidth.Get(context);
            var height = DialogHeight.Get(context);
            var placeFilter = PlaceFilter.Get(context);
            var operationCode = OperationCode.Get(context);
            var place = InvoicePlace.Get(context);
            var obj = (IWBPosInputWithPlaceListViewModel)IoC.Instance.Resolve(typeof(IWBPosInputWithPlaceListViewModel));
            var mandantId = MandantID.Get(context);
            obj.PrintTE = CheckPrintTE(mandantId);
            obj.PlaceFilter = placeFilter;
            obj.OperationCode = operationCode;
            obj.MandantId = mandantId;
            var isMigration = IsMigration.Get(context);
            obj.IsMigration = isMigration;

            obj.BatchcodeWorkflowCode = BatchcodeWorkflowCode.Get(context);
            obj.SkuChangeMpWorkflowCode = SkuChangeMpWorkflowCode.Get(context);

            var model = obj as ICustomListViewModel<IWBPosInput>;
            if (model == null)
                throw new DeveloperException("ViewModel doesn't implement ICustomListViewModel.");
            model.PanelCaption = Title;
            var source = Source.Get(context).Select(p => new IwbPosInputErrorInfo(p) {IsSelected = false});
            model.SetSource(new ObservableCollection<IWBPosInput>(source));
            obj.CurrentPlace = place;
            var viewService = IoC.Instance.Resolve<IViewService>();
            if (viewService.ShowDialogWindow(model, true, false, width, height) == true)
            {
                var result = model.GetSource() as IEnumerable<IWBPosInput>;
                if (result == null)
                    throw new DeveloperException("Source type is not IEnumerable.");

                if (obj.SelectedItems != null && obj.SelectedItems.Any())
                {
                    // пометим выбранные записи
                    foreach (var r in result)
                    {
                        r.IsSelected = obj.SelectedItems.Contains(r);
                    }
                }

                Result.Set(context, result.ToArray());
                InvoicePlace.Set(context, obj.SelectedPlace);
                DialogResult.Set(context, true);
            }
            else
            {
                DialogResult.Set(context, false);
            }
            PrintTE.Set(context, ((IWBPosInputWithPlaceListViewModel) model).PrintTE);
            var disposable = model as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        private bool CheckPrintTE(decimal? mandantCode)
        {
            if (mandantCode == null)
                return false;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<MandantGpv>>())
            {
                var values = mgr.GetFiltered(string.Format("GParamVal2Entity='MANDANT' and GParamValValue={0} and GlobalParamCode_r='{1}'", mandantCode, PrintTEOnInput)).ToArray();
                return values.Any() && values.First().GparamValValue.EqIgnoreCase("1");
            }
        }
    }
}
