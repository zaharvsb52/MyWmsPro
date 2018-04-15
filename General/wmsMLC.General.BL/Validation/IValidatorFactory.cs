namespace wmsMLC.General.BL.Validation
{
    public interface IValidatorFactory
    {
        bool IsNeedValidate(IValidatable item);

        IValidator CreateValidator(IValidatable item);
    }
}