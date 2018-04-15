/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSFastReportExport.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>19.09.2012 10:09:06</Date>
/// <Summary>Экспорт отчета Fast Report в файл</Summary>
/// --------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastReport;
using FastReport.Export;
using wmsMLC.General.Types;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Класс экпорта отчета FastReport в файл
    /// </summary>
    /// <typeparam name="T">тип объекта экспорта файла</typeparam>
    public class wmsEPSFastReportExport<T>
    {
        public delegate void EventErrorHandler(Exception ex, ErrorType errorType, string errorCode, string[] message, string businessProcess);
        public event EventErrorHandler OnEventErrorHandler;
        /// <summary>
        /// Полное имя файла (документа) для экпорта 
        /// </summary>
        string _fileName;
        /// <summary>
        /// Отчет Fast Report
        /// </summary>
        Report _report;

        /// <summary>
        /// Конструктор класса экпорта отчета
        /// </summary>
        /// <param name="report">имя файла отчета</param>
        /// <param name="fileName">имя файла, в который будет экпортирован отчет</param>
        public wmsEPSFastReportExport(Report report, string fileName)
        {
            _fileName = fileName;
            _report = report;
        }

        /// <summary>
        /// Экпорт отчета и запись его в файл
        /// </summary>
        public void ExportSave()
        {
            try
            {
                // Готовим отчет
                _report.Prepare();

                if (!(typeof(T) is IWmsEPSFastReportExport))
                {
                    
                    ///OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { ex.Message + "\r\n" + ex.InnerException.Message }, "Read_ConfigEPS")
                    return;
                }
                // Создаем экземпляр экспорта
                IWmsEPSFastReportExport export = (IWmsEPSFastReportExport)Activator.CreateInstance(typeof(T));
                // Экспортируем отчет
                export.Export(_report, _fileName);
            }
            catch (Exception ex)
            {
                OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { ex.Message + "\r\n" + ex.InnerException.Message }, "ExportReport");
            }

        }
        /// <summary>
        /// Экпорт отчета и запись его в файл
        /// </summary>
        /// <param name="fileName">показывать или нет окно просмотра настроек экпорта</param>
        public void ExportSave(bool previewExport)
        {
            try
            {
                // Готовим отчет
                _report.Prepare();
                if (!(typeof(T) is IWmsEPSFastReportExport))
                {
                    ///OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { ex.Message + "\r\n" + ex.InnerException.Message }, "Read_ConfigEPS")
                    return;
                }
                // Создаем экземпляр экспорта
                IWmsEPSFastReportExport export = (IWmsEPSFastReportExport)Activator.CreateInstance(typeof(T));

                // Показываем диалог с настройками экспорта и экспортируем отчет
                if (previewExport)
                {
                    if (export.ShowDialog()) 
                        export.Export(_report, _fileName);
                }
            }
            catch (Exception ex)
            {
                OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { ex.Message + "\r\n" + ex.InnerException.Message }, "ExportReport");
            }

        }

        
    }

    //class testBase
    //{
    //    public bool ShowDialog(FastReport.Export.ExportBase obj)
    //    {
    //        return obj.ShowDialog();
    //    }
    //}

}