namespace wmsMLC.Business.Objects
{
    public class TransportTaskType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CodePropertyName = "TTaskTypeCode";
        /*public const string NamePropertyName = "TransportTaskTypeName";
        public const string TareWeightPropertyName = "TransportTaskTypeTareWeight";
        public const string LengthPropertyName = "TransportTaskTypeLength";
        public const string WidthPropertyName = "TransportTaskTypeWidth";
        public const string HeightPropertyName = "TransportTaskTypeHeight";
        public const string MaxWeightPropertyName = "TransportTaskTypeMaxWeight";
        public const string LengthInternalPropertyName = "TransportTaskTypeLengthInternal";
        public const string WidthInternalPropertyName = "TransportTaskTypeWidthInternal";
        public const string HeightInternalPropertyName = "TransportTaskTypeHeightInternal";
        public const string NumberPrefixPropertyName = "TransportTaskTypeNumberPrefix";
        public const string TransportTaskTypeInsPropertyName = "TransportTaskTypeIns";
        public const string DateInsPropertyName = "DateIns";
        public const string TransportTaskTypeUpdPropertyName = "TransportTaskTypeUpd";
        public const string DateUpdPropertyName = "DateUpd";
        public const string SysParamsPropertyName = "SysParams";*/
        #endregion

        #region .  Properties  .
        public string TTaskTypeCode
        {
            get { return GetProperty<string>(CodePropertyName); }
            set { SetProperty(CodePropertyName, value); }
        }
        #endregion
    }
}