using System;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{

    /// <summary>
    /// Аттрибут позволяет указать dbhn
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class VirtualAttribute : RealyAllowMultipleAttribute
    {
        public VirtualAttribute(string virtualValue)
        {
            VirtualValue = virtualValue;
        }

        public string VirtualValue { get; private set; }

        public static string GetVirtualValue(Type type)
        {
            string value = null;
            var customAtts = type.GetCustomAttributes(typeof (VirtualAttribute), true);
            if (customAtts.Length == 1)
                value = ((VirtualAttribute) customAtts[0]).VirtualValue;
            if (customAtts.Length > 1)
                throw new DeveloperException(string.Format(DeveloperExceptionResources.DuplicateAttribute,  "VirtualValue"));
            return value;
        }

        public static string GetVirtualValue(object obj)
        {
            return GetVirtualValue(obj.GetType());
        }

    }
}