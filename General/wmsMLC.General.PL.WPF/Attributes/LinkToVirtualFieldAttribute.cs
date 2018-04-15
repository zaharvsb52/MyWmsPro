using System;

namespace wmsMLC.General.PL.WPF.Attributes
{
    /// <summary>
    /// Аттрибут, описывающий что для данного поля есть связака с виртуальным.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class LinkToVirtualFieldAttribute : RealyAllowMultipleAttribute
    {
        public LinkToVirtualFieldAttribute(string virtualFieldName)
        {
            VirtualFieldName = virtualFieldName;
        }

        public string VirtualFieldName { get; private set; }
    }
}