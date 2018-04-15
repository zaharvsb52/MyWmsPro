using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Acceptance.ViewModels;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Services;
using WebClient.Common.Types;

namespace wmsMLC.Activities.ViewInteraction
{
    public class AcceptanceEditActivity : NativeActivity<IWBPosInput[]>
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

        /// <summary>
        /// Код операции
        /// </summary>
        [RequiredArgument]
        [DisplayName(@"Накладная")]
        public InArgument<IWB> IWB { get; set; }

        [DisplayName(@"Признак, что накладная создана миграцией")]
        [DefaultValue(false)]
        public InArgument<bool> IsMigration { get; set; }

        [DisplayName(@"Показывать ли товар")]
        [DefaultValue(false)]
        public InOutArgument<bool> IsNeedShowProducts { get; set; }

        [DisplayName(@"Формат отображаемых полей")]
        [DefaultValue(null)]
        public InArgument<IDictionary<string, string>> DisplayFieldsFormat { get; set; }

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
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsNeedShowProducts, type.ExtractPropertyName(() => IsNeedShowProducts));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsMigration, type.ExtractPropertyName(() => IsMigration));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode, type.ExtractPropertyName(() => OperationCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IWB, type.ExtractPropertyName(() => IWB));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DisplayFieldsFormat, type.ExtractPropertyName(() => DisplayFieldsFormat));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var width = DialogWidth.Get(context);
            var height = DialogHeight.Get(context);
            var placeFilter = PlaceFilter.Get(context);
            var operationCode = OperationCode.Get(context);
            var place = InvoicePlace.Get(context);
            var iwb = IWB.Get(context);
            var mandantId = MandantID.Get(context);

            using (var viewModel = (AcceptanceViewModel) IoC.Instance.Resolve(typeof (AcceptanceViewModel)))
            {
                viewModel.PrintTE = CheckPrintTE(mandantId);
                viewModel.PlaceFilter = placeFilter;
                viewModel.OperationCode = operationCode;
                viewModel.MandantId = mandantId;
                viewModel.CurrentIWB = iwb;
                viewModel.IsMigration = IsMigration.Get(context);
                viewModel.BatchcodeWorkflowCode = BatchcodeWorkflowCode.Get(context);
                viewModel.SkuChangeMpWorkflowCode = SkuChangeMpWorkflowCode.Get(context);
                viewModel.PanelCaption = Title;
                viewModel.IsProductsShown = IsNeedShowProducts.Get(context);
                viewModel.DisplayFieldsFormat = DisplayFieldsFormat.Get(context);
                if (place != null)
                {
                    viewModel.AcceptancePlace = new EntityReference(place.PlaceCode, Place.EntityType,
                        new[]
                        {
                            new EntityReferenceFieldValue("PlaceCode", place.PlaceCode),
                            new EntityReferenceFieldValue("PlaceName", place.PlaceName)
                        });
                }

                ((IModelHandler)viewModel).SetSource(Source.Get(context));
                var viewService = IoC.Instance.Resolve<IViewService>();
                if (viewService.ShowDialogWindow(viewModel, true, false, width, height, noButtons: true) == true && viewModel.IsAllowAccept == true)
                {
                    if (viewModel.SelectedItems != null && viewModel.SelectedItems.Any())
                    {
                        // пометим выбранные записи
                        foreach (var r in viewModel.Source)
                            r.IsSelected = viewModel.SelectedItems.Contains(r);
                    }

                    Result.Set(context, viewModel.Source.Cast<IWBPosInput>().ToArray());

                    var acceptancePlace = GetAcceptancePlace(viewModel);
                    InvoicePlace.Set(context, acceptancePlace);
                    IsNeedShowProducts.Set(context, viewModel.IsProductsShown);
                    DialogResult.Set(context, true);
                }
                else
                {
                    DialogResult.Set(context, false);
                }

                PrintTE.Set(context, viewModel.PrintTE);
            }
        }

        private static Place GetAcceptancePlace(AcceptanceViewModel viewModel)
        {
            //Получаем место прямо из EntityReference, чтобы лишний раз не лезть в базу
            if (viewModel.AcceptancePlace == null)
                return null;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Place>>())
                return mgr.Get(viewModel.AcceptancePlace.Id, GetModeEnum.Partial);

            //var nameField = viewModel.AcceptancePlace.Values.FirstOrDefault(i => i.Name == "PlaceName");
            //return new Place { PlaceCode = viewModel.AcceptancePlace.Id.ToString(), PlaceName = nameField == null ? null : (string)nameField.Value};
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