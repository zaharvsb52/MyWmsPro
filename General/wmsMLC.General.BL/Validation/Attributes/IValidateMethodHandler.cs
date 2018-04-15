using System.ComponentModel;

namespace wmsMLC.General.BL.Validation.Attributes
{
    public interface IValidateMethodHandler
    {
        void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList);
    }
}