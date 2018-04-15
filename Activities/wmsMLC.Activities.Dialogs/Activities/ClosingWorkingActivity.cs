using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.Activities.Dialogs.Activities
{
    [DisplayName(@"Диалог закрытия выпоняемых работ")]
    public class ClosingWorkingActivity : NativeActivity<bool>
    {
        private const string WorkingIdPropertyName = "Работник";
        private readonly Dictionary<string, string> _wfCache;
        private NativeActivityContext _context;

        public ClosingWorkingActivity()
        {
            _wfCache = new Dictionary<string, string>();
            DisplayName = "Диалог закрытия выпоняемых работ";
            FontSize = 14;
            WorkerFullName = (string) null;
            WfDialogWorkerDateTill = (string) null;
            ExcludedOperations = (string) null;
        }

        #region .  Properties  .
        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(WorkingIdPropertyName)]
        public InArgument<decimal?> WorkerId { get; set; }

        [DisplayName(@"ФИО работника. Задается, если работник не Ваш")]
        public InArgument<string> WorkerFullName { get; set; }

        [DisplayName(@"Workflow. Диалог ввода времени завершения работы")]
        public InArgument<string> WfDialogWorkerDateTill { get; set; }

        [DisplayName(@"Список кодов операций (разделитель - запятая), которые не будут использоваться при проверке открытых выполнений работ")]
        public InArgument<string> ExcludedOperations { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, WorkerId, type.ExtractPropertyName(() => WorkerId));
            ActivityHelpers.AddCacheMetadata(collection, metadata, WorkerFullName, type.ExtractPropertyName(() => WorkerFullName));
            ActivityHelpers.AddCacheMetadata(collection, metadata, WfDialogWorkerDateTill, type.ExtractPropertyName(() => WfDialogWorkerDateTill));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ExcludedOperations, type.ExtractPropertyName(() => ExcludedOperations));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            const string isnullerrorformat = "Свойство '{0}' должно быть задано.";

            _context = context;

            var workerId = WorkerId.Get(context);
            if (!workerId.HasValue)
                throw new DeveloperException(isnullerrorformat, WorkingIdPropertyName);

            string filter = null;
            var excludedOperations = ExcludedOperations.Get(context);
            if (!string.IsNullOrEmpty(excludedOperations))
            {
                var ops = excludedOperations.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (ops.Length > 0)
                    filter = string.Format(" and w.operationcode_r not in ({0})",
                        string.Join(",", ops.Select(p => string.Format("'{0}'", p))));
            }

            var workhelper = new WorkHelper();
            var result = workhelper.ClosingWorking(workerId: workerId.Value, filter: filter, dialogTitle: "Подтверждение",
                workername: WorkerFullName.Get(context),
                dialogMessageHandler: ActivityHelpers.ClosingWorkingDialogMessage,
                dialogWorkerDateTillHandler: ShowDialogWorkerDateTill, fontSize: FontSize.Get(context));
            Result.Set(context, result);
        }

        private DateTime? ShowDialogWorkerDateTill()
        {
            const string dateTillPropertyName = "Value";

            var wfDialogWorkerDateTill = WfDialogWorkerDateTill.Get(_context);
            if (string.IsNullOrEmpty(wfDialogWorkerDateTill))
                return null;

            var workflowXaml = GetWorkflowXaml(wfDialogWorkerDateTill);
            DynamicActivity activity;
            using (var reader = new StringReader(workflowXaml))
                activity = (DynamicActivity)ActivityXamlServices.Load(reader);

            var bpContext = new BpContext();
            bpContext.Set("WorkerFullName", WorkerFullName.Get(_context));
            var inputs = new Dictionary<string, object> { { BpContext.BpContextArgumentName, bpContext } };
            WorkflowInvoker.Invoke(activity, inputs);

            if (inputs.ContainsKey(BpContext.BpContextArgumentName))
            {
                var bpCntx = inputs[BpContext.BpContextArgumentName] as BpContext;
                if (bpCntx != null && bpCntx.Properties.ContainsKey(dateTillPropertyName))
                    return bpCntx.Get<DateTime?>(dateTillPropertyName);
            }

            return null;
        }

        private string GetWorkflowXaml(string wfDialogWorkerDateTill)
        {
            if (_wfCache.ContainsKey(wfDialogWorkerDateTill))
                return _wfCache[wfDialogWorkerDateTill];

            _wfCache[wfDialogWorkerDateTill] = ActivityHelpers.GetWorkflowXaml(wfDialogWorkerDateTill);
            return _wfCache[wfDialogWorkerDateTill];
        }
        #endregion .  Methods  .
    }
}
