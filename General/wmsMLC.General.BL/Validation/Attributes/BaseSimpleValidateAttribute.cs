using System.ComponentModel;

namespace wmsMLC.General.BL.Validation.Attributes
{
    public abstract class BaseSimpleValidateAttribute : BaseValidateAttribute, IValidateMethodHandler
    {
        protected BaseSimpleValidateAttribute(int ordinal, string description, ValidateErrorLevel level)
            : base(ordinal)
        {
            Description = description;
            Level = level;
        }

        public string Description { get; private set; }

        public ValidateErrorLevel Level { get; private set; }

        public abstract void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList);
    }
}