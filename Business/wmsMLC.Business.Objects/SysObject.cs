using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Класс, содержащий описание метаданных объекта (описание класса, поля).
    /// </summary>
    public sealed class SysObject : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ObjectIDPropertyName = "OBJECTID";
        public const string ObjectParentIDPropertyName = "OBJECTPARENTID";
        public const string ObjectNamePropertyName = "OBJECTNAME";
        public const string ObjectDBNamePropertyName = "OBJECTDBNAME";
        public const string ObjectEntityCodePropertyName = "OBJECTENTITYCODE";
        public const string ObjectDataTypePropertyName = "OBJECTDATATYPE";
        public const string ObjectDefaultValuePropertyName = "OBJECTDEFAULTVALUE";
        public const string ObjectRelationshipPropertyName = "OBJECTRELATIONSHIP";
        public const string ObjectExtPropertyName = "OBJECTEXT";
        public const string ObjectLookupPropertyName = "OBJECTLOOKUPCODE_R";
        public const string ObjectFieldKeyLinkPropertyName = "OBJECTFIELDKEYLINK";
        public const string ObjectPKPropertyName = "OBJECTPK";
        public const string ObjectFieldLengthPropertyName = "OBJECTFIELDLENGTH";
        //public const string ObjectValidValuePropertyName = "ObjectValidValue";
        #endregion

        public SysObject()
        {
            UnknownPropertySet = UnknownPropertySetMode.Ignore;
        }

        #region .  Properties  .

        /// <summary> Уникальный идентификатор объекта </summary>
        [HardCodedProperty]
        [SourceName(ObjectIDPropertyName)]
        public decimal ObjectID
        {
            get { return GetProperty<decimal>(ObjectIDPropertyName); }
            set { SetProperty(ObjectIDPropertyName, value); }
        }

        /// <summary> Ссылка на родительский объект. Если данный объект является описание класса, то пусто. </summary>
        [HardCodedProperty]
        [SourceName(ObjectParentIDPropertyName)]
        public decimal? ObjectParentID
        {
            get { return GetProperty<decimal>(ObjectParentIDPropertyName); }
            set { SetProperty(ObjectParentIDPropertyName, value); }
        }

        /// <summary> Имя объекта </summary>
        [HardCodedProperty]
        [SourceName(ObjectNamePropertyName)]
        public string ObjectName
        {
            get { return GetProperty<string>(ObjectNamePropertyName); }
            set { SetProperty(ObjectNamePropertyName, value); }
        }

        /// <summary> Имя объекта для обмена со внешними системами (Service, DB) </summary>
        [HardCodedProperty]
        [SourceName(ObjectDBNamePropertyName)]
        public string ObjectDBName
        {
            get { return GetProperty<string>(ObjectDBNamePropertyName); }
            set { SetProperty(ObjectDBNamePropertyName, value); }
        }

        /// <summary> Код сущности родительского объекта </summary>
        [HardCodedProperty]
        [SourceName(ObjectEntityCodePropertyName)]
        public string ObjectEntityCode
        {
            get { return GetProperty<string>(ObjectEntityCodePropertyName); }
            set { SetProperty(ObjectEntityCodePropertyName, value); }
        }

        /// <summary> Код тип данных </summary>
        [HardCodedProperty]
        [SourceName(ObjectDataTypePropertyName)]
        public decimal? ObjectDataType
        {
            get { return GetProperty<decimal?>(ObjectDataTypePropertyName); }
            set { SetProperty(ObjectDataTypePropertyName, value); }
        }

        /// <summary> Значение по-умолчанию. Если значение задано, то данный объект автоматически не может быть null </summary>
        [HardCodedProperty]
        [SourceName(ObjectDefaultValuePropertyName)]
        public string ObjectDefaultValue
        {
            get { return GetProperty<string>(ObjectDefaultValuePropertyName); }
            set { SetProperty(ObjectDefaultValuePropertyName, value); }
        }

        /// <summary> Признак того, что данный объект - это коллекция объектов указанного типа </summary>
        [HardCodedProperty]
        [SourceName(ObjectRelationshipPropertyName)]
        public Relationship ObjectRelationship
        {
            get { return GetProperty<Relationship>(ObjectRelationshipPropertyName); }
            set { SetProperty(ObjectRelationshipPropertyName, value); }
        }

        /// <summary> Коллекция расширений объекта </summary>
        [HardCodedProperty]
        [SourceName(ObjectExtPropertyName)]
        public WMSBusinessCollection<SysObjectExt> ObjectExt
        {
            get { return GetProperty<WMSBusinessCollection<SysObjectExt>>(ObjectExtPropertyName); }
            set { SetProperty(ObjectExtPropertyName, value); }
        }

        /// <summary> Код связи данного объекта с другим (Lookup) </summary>
        [HardCodedProperty]
        [SourceName(ObjectLookupPropertyName)]
        public string ObjectLookupCode_r
        {
            get { return GetProperty<string>(ObjectLookupPropertyName); }
            set { SetProperty(ObjectLookupPropertyName, value); }
        }

        [HardCodedProperty]
        [SourceName(ObjectFieldKeyLinkPropertyName)]
        public string ObjectFieldKeyLink
        {
            get { return GetProperty<string>(ObjectFieldKeyLinkPropertyName); }
            set { SetProperty(ObjectFieldKeyLinkPropertyName, value); }
        }

        [HardCodedProperty]
        [SourceName(ObjectPKPropertyName)]
        public bool ObjectPK
        {
            get { return GetProperty<bool>(ObjectPKPropertyName); }
            set { SetProperty(ObjectPKPropertyName, value); }
        }

        /// <summary> Максимальная длина в поле </summary>
        [HardCodedProperty]
        [SourceName(ObjectFieldLengthPropertyName)]
        public decimal? ObjectFieldLength
        {
            get { return GetProperty<decimal?>(ObjectFieldLengthPropertyName); }
            set { SetProperty(ObjectFieldLengthPropertyName, value); }
        }
        #endregion

        protected override IValidator CreateValidator()
        {
            // В данном случае валидатор не нужен
            return null;
        }
    }

    public enum Relationship
    {
        One,
        Many
    }
}