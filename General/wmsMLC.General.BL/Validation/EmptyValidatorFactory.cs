namespace wmsMLC.General.BL.Validation
{
    public class EmptyValidatorFactory : IValidatorFactory
    {
        public IValidator CreateValidator(IValidatable item)
        {
            return null;
        }

        public bool IsNeedValidate(IValidatable item)
        {
            return false;
        }
    }
}