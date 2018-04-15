namespace wmsMLC.Business.Objects
{
    public class Sequence : WMSBusinessObject
    {
        #region .  Constants  .
        public const string SequenceCodePropertyName = "SequenceCode";
        #endregion

        #region .  Properties  .
        public string SequenceCode
        {
            get { return GetProperty<string>(SequenceCodePropertyName); }
            set { SetProperty(SequenceCodePropertyName, value); }
        }
        #endregion
    }
}