namespace wmsMLC.Business.Objects
{
    public class Place : WMSBusinessObject
    {
        #region .  Constants  .
        public const string EntityType = "WmsPlace";

        public const string SegmentCodePropertyName = "SEGMENTCODE_R";
        public const string PlaceNamePropertyName = "PLACENAME";
        public const string PlaceSPropertyName = "PLACES";
        public const string PlaceXPropertyName = "PLACEX";
        public const string PlaceYPropertyName = "PLACEY";
        public const string PlaceZPropertyName = "PLACEZ";
        public const string PlaceCheckNumberPropertyName = "PLACECHECKNUMBER";
        public const string PlaceCheckYNumberPropertyName = "PLACECHECKNUMBERY";
        public const string PlaceGroupCodePropertyName = "PLACEGROUPCODE";
        public const string PlaceWeightGroupPropertyName = "PLACEWEIGHTGROUP";
        public const string PlaceCapacityPropertyName = "PLACECAPACITY";
        public const string PlaceCapacityMaxPropertyName = "PLACECAPACITYMAX";
        public const string PlaceWeightPropertyName = "PLACEWEIGHT";
        public const string StatusCode_RPropertyName = "STATUSCODE_R";
        public const string PlaceClassCodePropertyName = "PLACECLASSCODE_R";
        public const string PlaceLengthPropertyName = "PLACELENGTH";
        public const string PlaceWidthPropertyName = "PLACEWIDTH";
        public const string PlaceHeightPropertyName = "PLACEHEIGHT";
        public const string PlaceTypeCodePropertyName = "PLACETYPECODE_R";
        public const string MotionAreaCode_RPropertyName = "MOTIONAREACODE_R";

        public const string PlaceSortAPropertyName = "PLACESORTA";
        public const string PlaceSortBPropertyName = "PLACESORTB";
        public const string PlaceSortCPropertyName = "PLACESORTC";
        public const string PlaceSortDPropertyName = "PLACESORTD";

        public const string SegmentCode_R_NumberPropertyName = "SEGMENTCODE_R_NUMBER";
        public const string AreaCodePropertyName = "VAREACODE";
        public const string AreaNamePropertyName = "VAREANAME";
        public const string WarehouseNamePropertyName = "VWAREHOUSENAME";
        public const string CustomParamValPropertyName = "CUSTOMPARAMVAL";

        public const string PlacePosXPropertyName = "PLACEPOSX";
        public const string PlacePosYPropertyName = "PLACEPOSY";

        #endregion .  Constants  .

        #region .  Properties  .
        public string PlaceCode
        {
            get { return GetProperty<string>(GetPrimaryKeyPropertyName()); }
            set { SetProperty(GetPrimaryKeyPropertyName(), value); }
        }
        public string SegmentCode
        {
            get { return GetProperty<string>(SegmentCodePropertyName); }
            set { SetProperty(SegmentCodePropertyName, value); }
        }
        public string PlaceName
        {
            get { return GetProperty<string>(PlaceNamePropertyName); }
            set { SetProperty(PlaceNamePropertyName, value); }
        }
        public string PlaceClassCode
        {
            get { return GetProperty<string>(PlaceClassCodePropertyName); }
            set { SetProperty(PlaceClassCodePropertyName, value); }
        }
        public decimal PlaceS
        {
            get { return GetProperty<decimal>(PlaceSPropertyName); }
            set { SetProperty(PlaceSPropertyName, value); }
        }
        public decimal PlaceX
        {
            get { return GetProperty<decimal>(PlaceXPropertyName); }
            set { SetProperty(PlaceXPropertyName, value); }
        }
        public decimal PlaceY
        {
            get { return GetProperty<decimal>(PlaceYPropertyName); }
            set { SetProperty(PlaceYPropertyName, value); }
        }
        public decimal PlaceZ
        {
            get { return GetProperty<decimal>(PlaceZPropertyName); }
            set { SetProperty(PlaceZPropertyName, value); }
        }
        public string PlaceCheck
        {
            get { return GetProperty<string>(PlaceCheckNumberPropertyName); }
            set { SetProperty(PlaceCheckNumberPropertyName, value); }
        }
        public string PlaceCheckY
        {
            get { return GetProperty<string>(PlaceCheckYNumberPropertyName); }
            set { SetProperty(PlaceCheckYNumberPropertyName, value); }
        }
        public string PlaceGroupCode
        {
            get { return GetProperty<string>(PlaceGroupCodePropertyName); }
            set { SetProperty(PlaceGroupCodePropertyName, value); }
        }
        public decimal PlaceWeightGroup
        {
            get { return GetProperty<decimal>(PlaceWeightGroupPropertyName); }
            set { SetProperty(PlaceWeightGroupPropertyName, value); }
        }
        public decimal PlaceCapacity
        {
            get { return GetProperty<decimal>(PlaceCapacityPropertyName); }
            set { SetProperty(PlaceCapacityPropertyName, value); }
        }
        public decimal PlaceCapacityMax
        {
            get { return GetProperty<decimal>(PlaceCapacityMaxPropertyName); }
            set { SetProperty(PlaceCapacityMaxPropertyName, value); }
        }
        public decimal PlaceWeight
        {
            get { return GetProperty<decimal>(PlaceWeightPropertyName); }
            set { SetProperty(PlaceWeightPropertyName, value); }
        }
        public decimal PlaceLength
        {
            get { return GetProperty<decimal>(PlaceLengthPropertyName); }
            set { SetProperty(PlaceLengthPropertyName, value); }
        }
        public decimal PlaceWidth
        {
            get { return GetProperty<decimal>(PlaceWidthPropertyName); }
            set { SetProperty(PlaceWidthPropertyName, value); }
        }
        public decimal PlaceHeight
        {
            get { return GetProperty<decimal>(PlaceHeightPropertyName); }
            set { SetProperty(PlaceHeightPropertyName, value); }
        }
        public string StatusCode_R
        {
            get { return GetProperty<string>(StatusCode_RPropertyName); }
            set { SetProperty(StatusCode_RPropertyName, value); }
        }
        public string PlaceTypeCode
        {
            get { return GetProperty<string>(PlaceTypeCodePropertyName); }
            set { SetProperty(PlaceTypeCodePropertyName, value); }
        }

        public string MotionAreaCode_R
        {
            get { return GetProperty<string>(MotionAreaCode_RPropertyName); }
            set { SetProperty(MotionAreaCode_RPropertyName, value); }
        }

        public decimal PlaceSortA
        {
            get { return GetProperty<decimal>(PlaceSortAPropertyName); }
            set { SetProperty(PlaceSortAPropertyName, value); }
        }

        public decimal PlaceSortB
        {
            get { return GetProperty<decimal>(PlaceSortBPropertyName); }
            set { SetProperty(PlaceSortBPropertyName, value); }
        }

        public decimal PlaceSortC
        {
            get { return GetProperty<decimal>(PlaceSortCPropertyName); }
            set { SetProperty(PlaceSortCPropertyName, value); }
        }

        public decimal PlaceSortD
        {
            get { return GetProperty<decimal>(PlaceSortDPropertyName); }
            set { SetProperty(PlaceSortDPropertyName, value); }
        }

        public string SegmentCode_R_Number
        {
            get { return GetProperty<string>(SegmentCode_R_NumberPropertyName); }
            set { SetProperty(SegmentCode_R_NumberPropertyName, value); }
        }
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
        public string WarehouseName
        {
            get { return GetProperty<string>(WarehouseNamePropertyName); }
            set { SetProperty(WarehouseNamePropertyName, value); }
        }

        public decimal PosX
        {
            get { return GetProperty<decimal>(PlacePosXPropertyName); }
            set { SetProperty(PlacePosXPropertyName, value); }
        }

        public decimal PosY
        {
            get { return GetProperty<decimal>(PlacePosYPropertyName); }
            set { SetProperty(PlacePosYPropertyName, value); }
        }

        public WMSBusinessCollection<PlaceCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<PlaceCpv>>(CustomParamValPropertyName); }
            set { SetProperty(CustomParamValPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}
