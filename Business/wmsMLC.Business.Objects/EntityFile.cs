namespace wmsMLC.Business.Objects
{
    public class EntityFile : WMSBusinessObject
    {
        #region .  Constants  .
        public const string File2EntityPropertyName = "FILE2ENTITY";
        public const string FileKeyPropertyName = "FILEKEY";
        public const string FileNamePropertyName = "FILENAME";
        public const string FileVersionPropertyName = "FILEVERSION";
        public const string FileDataPropertyName = "FILEDATA";
        #endregion .  Constants  .

        #region .  Properties  .
        public string File2Entity
        {
            get { return GetProperty<string>(File2EntityPropertyName); }
            set { SetProperty(File2EntityPropertyName, value); }
        }

        public string FileKey
        {
            get { return GetProperty<string>(FileKeyPropertyName); }
            set { SetProperty(FileKeyPropertyName, value); }
        }

        public string FileName
        {
            get { return GetProperty<string>(FileNamePropertyName); }
            set { SetProperty(FileNamePropertyName, value); }
        }

        public string FileVersion
        {
            get { return GetProperty<string>(FileVersionPropertyName); }
            set { SetProperty(FileVersionPropertyName, value); }
        }
       

        #endregion .  Properties  .
    }
}