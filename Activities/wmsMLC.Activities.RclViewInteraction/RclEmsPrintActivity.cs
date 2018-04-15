using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.Resources;

namespace wmsMLC.Activities.RclViewInteraction
{
    /// <summary>
    /// Подготовка данных для EPS.
    /// </summary>
    public class RclEmsPrintActivity : NativeActivity
    {
        public RclEmsPrintActivity()
        {
            DisplayName = "ТСД: Печать";
        }

        [DisplayName(@"Источник данных")]
        public InArgument<WMSBusinessObject> Source { get; set; }

        [DisplayName(@"Код отчета")]
        public InArgument<string> ReportCode { get; set; }

        [DisplayName(@"ШК")]
        public InArgument<string> Barcode { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ReportCode, type.ExtractPropertyName(() => ReportCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Barcode, type.ExtractPropertyName(() => Barcode));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var mgrReport = IoC.Instance.Resolve<IReport2EntityManager>();
            var bo = Source.Get(context);          
            var reportcode = ReportCode.Get(context);

            //var result = mgrReport.PrintReport(bo, reportcode, managerInstance == null ? null : managerInstance.GetMandantCode(bo), Barcode.Get(context));
            var result = mgrReport.PrintReport(bo, reportcode, Barcode.Get(context));

            if (result == null)
                throw new DeveloperException("PrintReportStatus is null.");
            var message = string.Format(StringResources.FormatForItem, bo,
                result.HasError
                    ? string.Format(StringResources.EpsJobCreateError, result.Error)
                    : string.Format(StringResources.EpsJobCreateOk, result.Job, reportcode, result.Printer));
            if (result.HasError && !string.IsNullOrEmpty(message))
            {
                var viewService = IoC.Instance.Resolve<IViewService>();
                viewService.ShowDialog(StringResources.ActionPrint
                    , message
                    , MessageBoxButton.OK
                    , MessageBoxImage.Information
                    , MessageBoxResult.Yes);
            }
        }
    }
}
