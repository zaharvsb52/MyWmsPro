using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    public class TEType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CodePropertyName = "TETypeCode";
        public const string TETypeNamePropertyName = "TETypeName";
        public const string TareWeightPropertyName = "TETypeTareWeight";
        public const string LengthPropertyName = "TETypeLength";
        public const string WidthPropertyName = "TETypeWidth";
        public const string HeightPropertyName = "TETypeHeight";
        public const string MaxWeightPropertyName = "TETypeMaxWeight";
        public const string TETYPE2MANDANTLPropertyName = "TETYPE2MANDANTL";
        public const string TETYPENUMBERPREFIXPropertyName = "TETYPENUMBERPREFIX";

        /*public const string LengthInternalPropertyName = "TETypeLengthInternal";
        public const string WidthInternalPropertyName = "TETypeWidthInternal";
        public const string HeightInternalPropertyName = "TETypeHeightInternal";
        public const string UserInsPropertyName = "UserIns";
        public const string DateInsPropertyName = "DateIns";
        public const string UserUpdPropertyName = "UserUpd";
        public const string DateUpdPropertyName = "DateUpd";
        public const string SysParamsPropertyName = "SysParams";*/
        #endregion .  Constants  .

        #region .  Properties  .
        public string TETypeCode
        {
            get { return GetProperty<string>(CodePropertyName); }
            set { SetProperty(CodePropertyName, value); }
        }
        public string TETypeName
        {
            get { return GetProperty<string>(TETypeNamePropertyName); }
            set { SetProperty(TETypeNamePropertyName, value); }
        }

        public decimal TareWeight
        {
            get { return GetProperty<decimal>(TareWeightPropertyName); }
            set { SetProperty(TareWeightPropertyName, value); }
        }
        public decimal Length
        {
            get { return GetProperty<decimal>(LengthPropertyName); }
            set { SetProperty(LengthPropertyName, value); }
        }
        public decimal Width
        {
            get { return GetProperty<decimal>(WidthPropertyName); }
            set { SetProperty(WidthPropertyName, value); }
        }
        public decimal Height
        {
            get { return GetProperty<decimal>(HeightPropertyName); }
            set { SetProperty(HeightPropertyName, value); }
        }
        public decimal MaxWeight
        {
            get { return GetProperty<decimal>(MaxWeightPropertyName); }
            set { SetProperty(MaxWeightPropertyName, value); }
        }

        public string TETYPENUMBERPREFIX
        {
            get { return GetProperty<string>(TETYPENUMBERPREFIXPropertyName); }
            set { SetProperty(TETYPENUMBERPREFIXPropertyName, value); }
        }

        [XmlNotIgnore]
        public WMSBusinessCollection<TEType2Mandant> TETYPE2MANDANTL
        {
            get { return GetProperty<WMSBusinessCollection<TEType2Mandant>>(TETYPE2MANDANTLPropertyName); }
            set { SetProperty(TETYPE2MANDANTLPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}
