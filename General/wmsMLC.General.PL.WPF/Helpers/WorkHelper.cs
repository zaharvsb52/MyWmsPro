using System;
using System.Data;
using System.Linq;
using System.Windows;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.General.PL.WPF.Helpers
{
    public class WorkHelper
    {
        public bool ClosingWorking(decimal workerId, string filter, string dialogTitle, string workername, Func<DataRow[], string, string> dialogMessageHandler, Func<DateTime?> dialogWorkerDateTillHandler = null, double? fontSize = null)
        {
            //Проверяем наличие открытых выполнений работ
            var workings = BPH.GetOpenWorkingsByWorkerId(workerId, filter);
            if (workings != null && workings.Rows.Count > 0)
            {
                var wrows = workings.Rows.Cast<DataRow>().ToArray();
                var message = dialogMessageHandler(wrows, workername);

                var vs = IoC.Instance.Resolve<IViewService>();
                var dr = vs.ShowDialog(dialogTitle
                    , message
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.Yes
                    , fontSize);

                if (dr == MessageBoxResult.No)
                    return false;

                //Закрываем
                var workingIds = wrows.Select(p => p["workingid"].To<decimal>()).ToArray();
                var dateTill = dialogWorkerDateTillHandler == null ? null : dialogWorkerDateTillHandler();
                
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    mgr.CompleteWorkings(workingIds, dateTill);
                }
            }

            return true;
        }
    }
}
