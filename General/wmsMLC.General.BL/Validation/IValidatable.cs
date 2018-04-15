namespace wmsMLC.General.BL.Validation
{
	public interface IValidatable
	{
		void Validate();
		IValidator Validator { get; }

		void SuspendValidating();
		void ResumeValidating();
	}
}
