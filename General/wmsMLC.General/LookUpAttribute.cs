using System;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class LookupAttribute : Attribute
    {
        public LookupAttribute(string lookUp, string keyLink)
        {
            this.LookUp = lookUp;
            this.KeyLink = keyLink;
        }
        public LookupAttribute(string lookUp)
        {
            this.LookUp = lookUp;
        }

        public string LookUp { get; private set; }
        public string KeyLink { get; private set; }

        public static string GetLookup(Type type)
        {
            string lookup = null;
            var customAtts = type.GetCustomAttributes(typeof (LookupAttribute), true);
            if (customAtts.Length == 1)
                lookup = ((LookupAttribute) customAtts[0]).LookUp;
            if (customAtts.Length > 1)
                throw new DeveloperException(string.Format(DeveloperExceptionResources.DuplicateAttribute,  "Lookup"));
            return lookup;
        }

        public static string GetLookup(object obj)
        {
            return GetLookup(obj.GetType());
        }

        public static string GetKeyLink(Type type)
        {
            string link = null;
            var customAtts = type.GetCustomAttributes(typeof(LookupAttribute), true);
            if (customAtts.Length == 1)
                link = ((LookupAttribute)customAtts[0]).KeyLink;
            if (customAtts.Length > 1)
                throw new DeveloperException(string.Format(DeveloperExceptionResources.DuplicateAttribute, "Lookup"));
            return link;
        }

        public static string GetKeyLink(object obj)
        {
            return GetKeyLink(obj.GetType());
        }
    }
}
