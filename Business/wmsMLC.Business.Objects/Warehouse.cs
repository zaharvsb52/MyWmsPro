namespace wmsMLC.Business.Objects
{
    public class Warehouse : WMSBusinessObject
    {
        #region .  Constants  .

        public const string WarehouseCodePropertyName = "WarehouseCode";
        public const string WarehouseNamePropertyName = "WarehouseName";
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";

        #endregion

        #region .  Properties  .

        public string WarehouseCode
        {
            get { return GetProperty<string>(WarehouseCodePropertyName); }
            set { SetProperty(WarehouseCodePropertyName, value); }
        }

        public string WarehouseName
        {
            get { return GetProperty<string>(WarehouseNamePropertyName); }
            set { SetProperty(WarehouseNamePropertyName, value); }
        }

        public WMSBusinessCollection<WarehouseCpv> CUSTOMPARAMVAL
        {
            get { return GetProperty<WMSBusinessCollection<WarehouseCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }

        #endregion
    }
}