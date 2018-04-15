namespace wmsMLC.Business.Objects
{
    public class UIButton : WMSBusinessObject
    {
        #region .  Constants  .
        public const string UIButtonCodePropertyName = "UIBUTTONCODE";
        public const string UIButtonPanelPropertyName = "UIBUTTONPANEL";
        public const string UIButtonParentPropertyName = "UIBUTTONPARENT";
        public const string UIButtonOrderPropertyName = "UIBUTTONORDER";
        public const string UIButtonCaptionPropertyName = "UIBUTTONCAPTION";
        public const string UIButtonHintPropertyName = "UIBUTTONHINT";
        public const string UIButtonHotKeyPropertyName = "UIBUTTONHOTKEY";
        public const string UIButtonImagePropertyName = "UIBUTTONIMAGE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string Code
        {
            get { return GetProperty<string>(UIButtonCodePropertyName); }
            set { SetProperty(UIButtonCodePropertyName, value); }
        }
        public string Panel
        {
            get { return GetProperty<string>(UIButtonPanelPropertyName); }
            set { SetProperty(UIButtonPanelPropertyName, value); }
        }
        public string Parent
        {
            get { return GetProperty<string>(UIButtonParentPropertyName); }
            set { SetProperty(UIButtonParentPropertyName, value); }
        }
        public decimal Order
        {
            get { return GetProperty<decimal>(UIButtonOrderPropertyName); }
            set { SetProperty(UIButtonOrderPropertyName, value); }
        }
        public string Hint
        {
            get { return GetProperty<string>(UIButtonHintPropertyName); }
            set { SetProperty(UIButtonHintPropertyName, value); }
        }
        public string Caption
        {
            get { return GetProperty<string>(UIButtonCaptionPropertyName); }
            set { SetProperty(UIButtonCaptionPropertyName, value); }
        }
        public string HotKey
        {
            get { return GetProperty<string>(UIButtonHotKeyPropertyName); }
            set { SetProperty(UIButtonHotKeyPropertyName, value); }
        }
        public string Image
        {
            get { return GetProperty<string>(UIButtonImagePropertyName); }
            set { SetProperty(UIButtonImagePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}