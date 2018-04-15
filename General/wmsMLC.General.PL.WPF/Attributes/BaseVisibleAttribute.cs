using System;

namespace wmsMLC.General.PL.WPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public abstract class BaseVisibleAttribute : RealyAllowMultipleAttribute
    {
        public BaseVisibleAttribute(bool visible)
        {
            Visible = visible;
        }

        public bool Visible { get; private set; }
    }
}