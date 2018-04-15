namespace wmsMLC.Business.Objects
{
    public class InputPlPos : WMSBusinessObject
    {
        #region .  Consts  .
        public const string StatuscodePropertyname = "STATUSCODE_R";
        public const string InputplposcountmanPropertyName = "INPUTPLPOSCOUNTMAN";
        public const string InputplpostemanPropertyName = "INPUTPLPOSTEMAN";
        public const string PlIdPropertyName = "PLID_R";
        public const string PlaceCode_rPropertyName = "PLACECODE_R";
        public const string INPUTPLPOSSORTPropertyName = "INPUTPLPOSSORT";
        public const string SKUID_rPropertyName = "SKUID_R";
        public const string INPUTPLPOSCOUNTSKUPLANPropertyName = "INPUTPLPOSCOUNTSKUPLAN";
        public const string INPUTPLPOSCOUNTSKUFACTPropertyName = "INPUTPLPOSCOUNTSKUFACT";
        #endregion .  Consts  .

        public InputPlPos() { }

        public InputPlPos(PLPos plpos)
        {
            if (plpos == null)
                return;

            SetProperty(GetPrimaryKeyPropertyName(), plpos.GetKey());
            SetProperty(PlIdPropertyName, plpos.PLID_R);
            SetProperty(StatuscodePropertyname, plpos.StatusCode_r);
            SetProperty(PlaceCode_rPropertyName, plpos.PlaceCode_r);
            SetProperty(INPUTPLPOSSORTPropertyName, plpos.PLPosSort);
            SetProperty(SKUID_rPropertyName, plpos.SKUID_r);
            SetProperty(INPUTPLPOSCOUNTSKUPLANPropertyName, plpos.PLPosCountSKUPlan);
            SetProperty(INPUTPLPOSCOUNTSKUFACTPropertyName, plpos.PLPosCountSKUFact);
            SetProperty(InputplposcountmanPropertyName, plpos.PLPosCountSKUPlan - plpos.PLPosCountSKUFact);
            SetProperty(InputplpostemanPropertyName, string.Empty);
        }

        public decimal PlIdR
        {
            get { return GetProperty<decimal>(PlIdPropertyName); }
            set { SetProperty(PlIdPropertyName, value); }
        }
    }
}