namespace wmsMLC.Business.Objects
{
    public class PrinterLogical : WMSBusinessObject
    {
        #region .  Constants  .
        public const string LOGICALPRINTERPropertyName = "LOGICALPRINTER";
        public const string LogicalPrinterLockedPropertyName = "LOGICALPRINTERLOCKED";
        public const string PhysicalPrinter_RPropertyName = "PHYSICALPRINTER_R";
        public const string LogicalPrinterCopiesPropertyName = "LOGICALPRINTERCOPIES";
        public const string LogicalPrinterBarCodePropertyName = "LOGICALPRINTERBARCODE";
        #endregion .  Constants  .

        #region .  Properties  .
        public bool LogicalPrinterLocked
        {
            get { return GetProperty<bool>(LogicalPrinterLockedPropertyName); }
            set { SetProperty(LogicalPrinterLockedPropertyName, value); }
        }

        public string PhysicalPrinter_R
        {
            get { return GetProperty<string>(PhysicalPrinter_RPropertyName); }
            set { SetProperty(PhysicalPrinter_RPropertyName, value); }
        }

        public decimal LogicalPrinterCopies
        {
            get { return GetProperty<decimal>(LogicalPrinterCopiesPropertyName); }
            set { SetProperty(LogicalPrinterCopiesPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}