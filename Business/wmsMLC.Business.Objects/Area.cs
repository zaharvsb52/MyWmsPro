namespace wmsMLC.Business.Objects
{
    public class Area : WMSBusinessObject
    {
        #region .  Constants  .
        public const string AreaCodePropertyName = "AREACODE";
        public const string AreaNamePropertyName = "AREANAME";
        public const string WarehousePropertyName = "WAREHOUSECODE_R";

        #endregion

        #region .  Properties  .
        public string AreaCode
        {
            get { return GetProperty<string>(AreaCodePropertyName); }
            set { SetProperty(AreaCodePropertyName, value); }
        }

        public string AreaName
        {
            get { return GetProperty<string>(AreaNamePropertyName); }
            set { SetProperty(AreaNamePropertyName, value); }
        }

        public string Warehouse
        {
            get { return GetProperty<string>(WarehousePropertyName); }
            set { SetProperty(WarehousePropertyName, value); }
        }
        #endregion
    }
}
