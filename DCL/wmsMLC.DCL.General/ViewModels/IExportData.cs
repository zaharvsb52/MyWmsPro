using System;
using System.IO;

namespace wmsMLC.DCL.General.ViewModels
{
    /// <summary>
    /// Интерфейс для экпорта данных
    /// </summary>
    public interface IExportData
    {
        /// <summary>
        /// Событие - экспорт данных
        /// </summary>
        event EventHandler SourceExport;
       
        /// <summary>
        /// Stream - экпортируемые данные
        /// </summary>
        Stream StreamExport { get; set; }
    }
}