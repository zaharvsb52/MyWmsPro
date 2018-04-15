namespace wmsMLC.Business.Objects
{
    public class PrinterPhysical : WMSBusinessObject
    {
        #region .  Constants  .
        public const string PhysicalPrinterLockedPropertyName = "PHYSICALPRINTERLOCKED";
        #endregion .  Constants  .

        #region .  Properties  .
        public bool PhysicalPrinterLocked
        {
            get { return GetProperty<bool>(PhysicalPrinterLockedPropertyName); }
            set { SetProperty(PhysicalPrinterLockedPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}