using System;

namespace wmsMLC.General.BL
{
    /// <summary>
    /// Аттрибут, описывающий поле - глобальный конфигуратор. 
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GCFieldAttribute : RealyAllowMultipleAttribute
    {
        public GCFieldAttribute()
        {
        }

    }
}