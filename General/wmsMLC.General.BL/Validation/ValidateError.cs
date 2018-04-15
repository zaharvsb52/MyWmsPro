namespace wmsMLC.General.BL.Validation
{
    public class ValidateError
    {
        #region .  Helpers  .

        public static ValidateError Info(string description)
        {
            return new ValidateError(description, ValidateErrorLevel.Information);
        }

        public static ValidateError Warning(string description)
        {
            return new ValidateError(description, ValidateErrorLevel.Warning);
        }

        public static ValidateError Critical(string description)
        {
            return new ValidateError(description, ValidateErrorLevel.Critical);
        }

        #endregion

        public ValidateError(string description, ValidateErrorLevel level)
        {
            Description = description;
            Level = level;
        }

        public ValidateErrorLevel Level { get; private set; }

        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != typeof(ValidateError))
                return false;

            return Equals((ValidateError)obj);
        }

        public bool Equals(ValidateError other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Equals(other.Level, Level) && Equals(other.Description, Description);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Level.GetHashCode() * 397) ^ (Description != null ? Description.GetHashCode() : 0);
            }
        }
    }
}