/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSPrinterStatus.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>03.10.2012 12:56:59</Date>
/// <Summary>Проверка статусов принтеров, установленных в системе</Summary>
/// --------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using wmsMLC.General.Types;
using wmsMLC.General.wmsLogClient;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Объект отслеживания статусов принтеров в системе
    /// </summary>
    public class wmsEPSPrinterStatus : wmsBaseObject, IDisposable
    {
        /// <summary>
        /// Энумерация статусов принтера
        /// </summary>
        private enum PrinterStatus { Invalid, OK, Error, Degraded, Unknown, PredFail, Starting, Stopping, Service, Stressed, NonRecover, NoContact, LostComm };        

        /// <summary>
        /// Событие отправляемого сообщения
        /// </summary>
        //public event EventErrorHandler OnEventErrorHandler;

        /// <summary>
        /// Экземпляр объекта класса
        /// </summary>
        private static wmsEPSPrinterStatus _instance;

        /// <summary>
        /// Объект события изменений статусов принтеров
        /// </summary>
        private ManagementEventWatcher _watcher;

        /// <summary>
        /// Бибилиотека принтеров в системе
        /// </summary>
        private IDictionary<string, ManagementBaseObject> _printers = new Dictionary<string, ManagementBaseObject>();

        /// <summary>
        /// Бибилотека статус сообщений
        /// </summary>
        private IDictionary<string, PrinterStatus> _statusMessages = new Dictionary<string, PrinterStatus>();

        /// <summary>
        /// Библиотека кодов ошибок
        /// </summary>
        private IDictionary<UInt16, string> _ErrorStateMessages = new Dictionary<UInt16, string>();

        /// <summary>
        /// Конструктор класса
        /// </summary>
        private wmsEPSPrinterStatus(string linkById)
        {
            _linkById = linkById;
            InitMessages();
            InitPrinters();
            string wqlQuery = @"SELECT * FROM __InstanceModificationEvent WITHIN 3 WHERE TargetInstance ISA 'Win32_Printer'";
            WqlEventQuery query = new WqlEventQuery(wqlQuery);
            _watcher = new ManagementEventWatcher(query);
            _watcher.EventArrived += new EventArrivedEventHandler(onEvent);
            _watcher.Start();
        }

        /// <summary>
        /// Инициализация библиотеки принтеров в системе
        /// </summary>
        private void InitPrinters()
        {
            try
            {
                string query = string.Format("SELECT * from Win32_Printer");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection coll = searcher.Get();
                foreach (ManagementObject printer in coll)
                {
                    _printers.Add(printer["DeviceID"].ToString(), printer);
                }
            }
            catch (Exception ex)
            {
                wmsLogClient.Logger.OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { ex.Message }, "EPS_PRINTER_STATUS_INIT_PRINTERS", _linkById); 
            }
        }

        /// <summary>
        /// Инициализация библиотек сообщений и кодов ошибок принтера
        /// </summary>
        private void InitMessages()
        {
            _statusMessages.Add("Invalid", PrinterStatus.Invalid);
            _statusMessages.Add("OK", PrinterStatus.OK);
            _statusMessages.Add("Error", PrinterStatus.Error);
            _statusMessages.Add("Degraded", PrinterStatus.Degraded);
            _statusMessages.Add("Unknown", PrinterStatus.Unknown);
            _statusMessages.Add("Pred Fail", PrinterStatus.PredFail);
            _statusMessages.Add("Starting", PrinterStatus.Starting);
            _statusMessages.Add("Stopping", PrinterStatus.Stopping);
            _statusMessages.Add("Service", PrinterStatus.Service);
            _statusMessages.Add("Stressed", PrinterStatus.Stressed);
            _statusMessages.Add("NonRecover", PrinterStatus.NonRecover);
            _statusMessages.Add("No Contact", PrinterStatus.NoContact);
            _statusMessages.Add("Lost Comm", PrinterStatus.LostComm);

            _ErrorStateMessages.Add(0, "Unknown");
            _ErrorStateMessages.Add(1, "Other");
            _ErrorStateMessages.Add(2, "No Error");
            _ErrorStateMessages.Add(3, "Low Paper");
            _ErrorStateMessages.Add(4, "No Paper");
            _ErrorStateMessages.Add(5, "Low Toner");
            _ErrorStateMessages.Add(6, "No Toner");
            _ErrorStateMessages.Add(7, "Door Open");
            _ErrorStateMessages.Add(8, "Jammed");
            _ErrorStateMessages.Add(9, "Offline");
            _ErrorStateMessages.Add(10, "Service Requested");
            _ErrorStateMessages.Add(11, "Output Bin Full");
        }

        /// <summary>
        /// Получение экземпляра объекта класса
        /// </summary>
        /// <returns>объект класса</returns>
        public static wmsEPSPrinterStatus Instance(string linkById)
        {

            if (_instance == null)
            {
                _instance = new wmsEPSPrinterStatus(linkById);
            }
            return _instance;
        }

        /// <summary>
        /// Очистка объекта
        /// </summary>
        public void Dispose()
        {
            _watcher.Stop();
            _watcher.EventArrived -= new EventArrivedEventHandler(onEvent);
            _watcher.Dispose();
            _printers.Clear();
            _statusMessages.Clear();
            _ErrorStateMessages.Clear();
        }

        /// <summary>
        /// Событие изменения статуса принтеров
        /// </summary>
        /// <param name="sender">объект слежки событий</param>
        /// <param name="e">аргументы события</param>
        private void onEvent(object sender, EventArrivedEventArgs e)
        {
            try
            {
                foreach (PropertyData p in e.NewEvent.Properties)
                {
                    if (p.Name.Equals("TargetInstance"))
                    {
                        UpdatePrinter((ManagementBaseObject)p.Value);                        
                    }
                }
            }
            catch (Exception ex)
            {
                wmsLogClient.Logger.OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { ex.Message }, "EPS_PRINTER_STATUS_EVENT", _linkById); 
            }
        }

        /// <summary>
        /// Обновление статуса принтера
        /// </summary>
        /// <param name="mbo">объект параметров принтера</param>
        private void UpdatePrinter(ManagementBaseObject mbo)
        {
            try
            {                
                ManagementBaseObject tmp = _printers[mbo["DeviceID"].ToString()];
                _printers[mbo["DeviceID"].ToString()] = mbo;
                UInt16 state = Convert.ToUInt16(mbo["DetectedErrorState"]);
                string status = mbo["Status"].ToString();
                wmsLogClient.Logger.OnEventErrorHandler(null, ErrorType.INFO, "EpsPrinterStatus", new string[] { mbo["DeviceID"].ToString(), status, _ErrorStateMessages[state] }, "EPS_PRINTER_STATUS_EVENT", _linkById);
            }
            catch (Exception ex)
            {
                _printers.Add(new KeyValuePair<string, ManagementBaseObject>(mbo["DeviceID"].ToString(), mbo));
                wmsLogClient.Logger.OnEventErrorHandler(ex, ErrorType.INFO, "EpsPrinterAdd", new string[] { mbo["DeviceID"].ToString() }, "EPS_PRINTER_STATUS_EVENT", _linkById);
            }
        }

        /// <summary>
        /// Получение статуса принтера
        /// </summary>
        /// <param name="printer">имя принтера в системе</param>
        /// <returns>true в случае работоспособности принтера</returns>
        public bool GetStatus(string printer)
        {
            bool ps = true;
            try
            {
                ManagementBaseObject tmp = _printers[printer];
                PrinterStatus s = _statusMessages[tmp["Status"].ToString()];
                if (s == PrinterStatus.Error | s == PrinterStatus.LostComm | s == PrinterStatus.NoContact | s == PrinterStatus.NonRecover)
                {
                    ps = false;
                }
            }
            catch (Exception ex)
            {
                wmsLogClient.Logger.OnEventErrorHandler(ex, ErrorType.ERROR, "EEPSNoPrinter", new string[] { printer }, "EPS_PRINTER_STATUS", _linkById);
                ps = false;
            }
            return ps;
        }
    }
}
