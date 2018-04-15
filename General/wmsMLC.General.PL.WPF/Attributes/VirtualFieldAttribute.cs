using System;

namespace wmsMLC.General.PL.WPF.Attributes
{
    /// <summary>
    /// Аттрибут, описывающий виртальное поле. Его наличие говорит о том, что поле виртуальное. Так же в нем может быть указана ссылка на связанное реальное поле.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class VirtualFieldAttribute : RealyAllowMultipleAttribute
    {
        public VirtualFieldAttribute() { }
        public VirtualFieldAttribute(string parentFieldName)
        {
            ParentFieldName = parentFieldName;
        }

        public string ParentFieldName { get; set; }
    }
}