namespace wmsMLC.Business.Objects
{
    public class ObjectTreeMenu : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ObjectTreeCodePropertyName = "ObjectTreeCode";
        public const string ObjectTreeParentPropertyName = "ObjectTreeParent";
        public const string ObjectTreeNamePropertyName = "ObjectTreeName";
        public const string ObjectTreeActionPropertyName = "ObjectTreeAction";
        public const string ObjectTreePictureSmallPropertyName = "ObjectTreePictureSmall";
        public const string ObjectTreePictureLargePropertyName = "ObjectTreePictureLarge";
        public const string ObjectTreeOrderPropertyName = "OBJECTTREEORDER";
        #endregion

        #region .  Properties  .
        public decimal ObjectTreeOrder
        {
            get { return GetProperty<decimal>(ObjectTreeOrderPropertyName); }
            set { SetProperty(ObjectTreeOrderPropertyName, value); }
        }

        public string ObjectTreeCode
        {
            get { return GetProperty<string>(ObjectTreeCodePropertyName); }
            set { SetProperty(ObjectTreeCodePropertyName, value); }
        }

        public string ObjectTreeParent
        {
            get { return GetProperty<string>(ObjectTreeParentPropertyName); }
            set { SetProperty(ObjectTreeParentPropertyName, value); }
        }

        public string ObjectTreeName
        {
            get { return GetProperty<string>(ObjectTreeNamePropertyName); }
            set { SetProperty(ObjectTreeNamePropertyName, value); }
        }

        public string ObjectTreeAction
        {
            get { return GetProperty<string>(ObjectTreeActionPropertyName); }
            set { SetProperty(ObjectTreeActionPropertyName, value); }
        }

        public string ObjectTreePictureSmall
        {
            get { return GetProperty<string>(ObjectTreePictureSmallPropertyName); }
            set { SetProperty(ObjectTreePictureSmallPropertyName, value); }
        }

        public string ObjectTreePictureLarge
        {
            get { return GetProperty<string>(ObjectTreePictureLargePropertyName); }
            set { SetProperty(ObjectTreePictureLargePropertyName, value); }
        }
        #endregion
    }
}
