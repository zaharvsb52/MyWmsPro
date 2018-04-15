using System;

namespace wmsMLC.General.PL.WPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public abstract class BaseEnableAttribute : RealyAllowMultipleAttribute
    {
        public BaseEnableAttribute(bool enable)
        {
            Enable = enable;
        }

        public bool Enable { get; private set; }
    }
}