using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    [SourceName("GlobalParamValue")]
    public class PartnerGpv : GlobalParamValue
    {
        #region .  Constants  .
        public new const string GParamIDPropertyName = "GPARAMID";
        public new const string GParamVal2EntityPropertyName = "GPARAMVAL2ENTITY";
        public new const string GlobalParamCodePropertyName = "GLOBALPARAMCODE_R";
        public new const string GParamValKeyPropertyName = "GPARAMVALKEY";
        public new const string GparamValValuePropertyName = "GPARAMVALVALUE";
        #endregion .  Constants  .

        #region .  Properties  .
        public new string GlobalParamCode_R
        {
            get { return GetProperty<string>(ChangePropertyName(GlobalParamCodePropertyName)); }
            set { SetProperty(ChangePropertyName(GlobalParamCodePropertyName), value); }
        }

        public new string GparamValValue
        {
            get { return GetProperty<string>(ChangePropertyName(GparamValValuePropertyName)); }
            set { SetProperty(ChangePropertyName(GparamValValuePropertyName), value); }
        }

        public new string GParamVal2Entity
        {
            get { return GetProperty<string>(ChangePropertyName(GParamVal2EntityPropertyName)); }
            set { SetProperty(ChangePropertyName(GParamVal2EntityPropertyName), value); }
        }

        public new decimal GParamValKey
        {
            get { return GetProperty<decimal>(ChangePropertyName(GParamValKeyPropertyName)); }
            set { SetProperty(ChangePropertyName(GParamValKeyPropertyName), value); }
        }

        public new decimal GParamID
        {
            get { return GetProperty<decimal>(ChangePropertyName(GParamIDPropertyName)); }
            set { SetProperty(ChangePropertyName(GParamIDPropertyName), value); }
        }
        #endregion .  Properties  .
    }
}