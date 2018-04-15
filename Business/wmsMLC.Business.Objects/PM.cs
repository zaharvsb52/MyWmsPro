namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Сущность 'Менеджер товара'.
    /// </summary>
    public class PM : WMSBusinessObject
    {
        #region .  Constants  .

        public const string PM2OPERATIONLPropertyName = "PM2OPERATIONL";

        #endregion .  Constants  .

        #region .  Properties  .
        
        public WMSBusinessCollection<PM2Operation> PM2OPERATIONL
        {
            get { return GetProperty<WMSBusinessCollection<PM2Operation>>(PM2OPERATIONLPropertyName); }
            set { SetProperty(PM2OPERATIONLPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}