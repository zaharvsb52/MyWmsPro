using System.ComponentModel;

namespace wmsMLC.General.BL.Validation.Attributes
{
    /// <summary>
    /// Валидатор. Элемент не может ссылаться сам на себя.
    /// </summary>
    public class ValidateParentReferenceAttribute : BaseSimpleValidateAttribute
    {
        #region .  Constants  .

        public const string DefaultDescription = "Элемент не может ссылаться сам на себя";
        private const ValidateErrorLevel DefaultErrorLevel = ValidateErrorLevel.Critical;

        #endregion .  Constants  .

        public ValidateParentReferenceAttribute(string childPropertyName = null, int ordinal = DefaultOrdinal,
            string description = DefaultDescription, ValidateErrorLevel level = DefaultErrorLevel)
            : base(ordinal, description, level)
        {
            ChildPropertyName = childPropertyName;
        }

        public string ChildPropertyName { get; private set; }

        public override void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
        {
            var parentvalue = propInfo.GetValue(obj);
            if (parentvalue == null)
                return;

            object childvalue = null;
            if (string.IsNullOrEmpty(ChildPropertyName))
            {
                childvalue = ((IKeyHandler) obj).GetKey();
            }
            else
            {
                var propDescrs = TypeDescriptor.GetProperties(obj);
                var childprop = propDescrs.Find(ChildPropertyName, true);
                if (childprop != null)
                    childvalue = childprop.GetValue(obj);
            }
            if (childvalue == null)
                return;

            if (Equals(childvalue, parentvalue))
                errorsList.Add(new ValidateError(Description, Level));
        }
    }
}
