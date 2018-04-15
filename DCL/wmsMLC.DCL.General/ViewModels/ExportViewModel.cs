using System;
using System.IO;

namespace wmsMLC.DCL.General.ViewModels
{
    public class ExportViewModel<T> : ListViewModelBase<T>, IExportData
    {
        public event EventHandler SourceExport;
        
        public Stream StreamExport { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (StreamExport != null)
                    StreamExport.Dispose();
            base.Dispose(disposing);
        }
    }
}