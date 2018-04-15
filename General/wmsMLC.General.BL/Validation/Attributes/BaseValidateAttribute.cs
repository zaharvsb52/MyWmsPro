using System;

namespace wmsMLC.General.BL.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class BaseValidateAttribute : RealyAllowMultipleAttribute
    {
        protected const int DefaultOrdinal = 0;

        protected BaseValidateAttribute(int ordinal)
        {
            Ordinal = ordinal;
        }

        public int Ordinal { get; private set; }
    }
}
