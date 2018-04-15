using System;

namespace wmsMLC.General.PL.WPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public abstract class BaseFormatAttribute : RealyAllowMultipleAttribute
    {
        public BaseFormatAttribute(string displayFormat)
        {
            DisplayFormat = displayFormat;
        }

        public string DisplayFormat { get; private set; }
    }
}