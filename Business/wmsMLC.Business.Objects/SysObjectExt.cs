using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.Business.Objects
{
    [SourceName("OBJECTEXT")]
    public class SysObjectExt : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ObjectNamePropertyName = "OBJECTNAME";
        public const string AttrNamePropertyName = "ATTRNAME";
        public const string AttrValuePropertyName = "ATTRVALUE";
        #endregion

        #region .  Properties  .
        /// <summary>
        /// Имя объекта, к которому относится данное расширение
        /// </summary>
        [HardCodedProperty]
        [SourceName("OBJECTNAME")]
        public string ObjectName
        {
            get { return GetProperty<string>(ObjectNamePropertyName); }
            set { SetProperty(ObjectNamePropertyName, value); }
        }

        /// <summary>
        /// Имя
        /// </summary>
        [HardCodedProperty]
        [SourceName("ATTRNAME")]
        public string AttrName
        {
            get { return GetProperty<string>(AttrNamePropertyName); }
            set { SetProperty(AttrNamePropertyName, value); }
        }

        /// <summary>
        /// Значение
        /// </summary>
        [HardCodedProperty]
        [SourceName("ATTRVALUE")]
        public string AttrValue
        {
            get { return GetProperty<string>(AttrValuePropertyName); }
            set { SetProperty(AttrValuePropertyName, value); }
        } 
        #endregion

        public override string ToString()
        {
            var displayName = GetType().GetDisplayName();
            return displayName + (ObjectName == null ? string.Empty : string.Format(" '{0}'", ObjectName));
        }

        public override bool HasPrimaryKey()
        {
            return false;
        }

        protected override IValidator CreateValidator()
        {
            // В данном случае валидатор не нужен
            return null;
        }
    }
}