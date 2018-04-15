using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wmsMLC.General.BL.Validation
{
	public class ValidateErrorsList : List<ValidateError>
	{
		#region .  Constants  .

		public const string ErrorDescriptionNotSetTemplate = "Description not set";

		#endregion

		#region .  Methods  .

		public bool Contains(string description)
		{
			return this.Any(t => t.Description.Equals(description));
		}

		public void RemoveByDescription(string description)
		{
			var res = this
				.Where(t => (description == null && t.Description == null) ||
							(t.Description != null && t.Description.Equals(description)))
				.ToList();

			foreach (var r in res)
				Remove(r);
		}

		public bool HasCriticalErrors
		{
			get { return HasErrorsWithLevel(ValidateErrorLevel.Critical); }
		}

		public string GetCriticalErrorDescription()
		{
			var sb = new StringBuilder();
			foreach (var error in this.Where(error => error.Level == ValidateErrorLevel.Critical))
			{
				sb.AppendLine(!string.IsNullOrEmpty(error.Description)
								? error.Description
								: ErrorDescriptionNotSetTemplate);
			}
			return sb.ToString();
		}

		public bool HasErrorsWithLevel(ValidateErrorLevel level)
		{
			return this.Any(i => i.Level == level);
		}

		public bool HasErrorsWithDescription(string description)
		{
			return this.Any(i => i.Description == description);
		}

		public ValidateErrorLevel GetMaxErrorLevel()
		{
			return Count == 0
					? ValidateErrorLevel.None
					: this.Max(i => i.Level);
		}

		public string GetErrorsDescription()
		{
			if (Count == 0)
				return null;

			var sb = new StringBuilder();

			foreach (var error in this.OrderByDescending(i => i.Level))
			{
				sb.AppendLine(!string.IsNullOrEmpty(error.Description)
								? error.Description
								: ErrorDescriptionNotSetTemplate);
			}

			return sb.ToString();
		}

		public override string ToString()
		{
			//TODO: УБРАТЬ! Нужно будет проверить все неявные приведения
			return GetErrorsDescription();
		}

		#endregion
	}
}