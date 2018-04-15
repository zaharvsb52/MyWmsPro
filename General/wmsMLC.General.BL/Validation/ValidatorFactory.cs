
namespace wmsMLC.General.BL.Validation
{
    public class ValidatorFactory : IValidatorFactory
    {
        public IValidator CreateValidator(IValidatable item)
        {
            return new BlankValidator(item);
        }

        public bool IsNeedValidate(IValidatable item)
        {
            return true;
        }
    }
}