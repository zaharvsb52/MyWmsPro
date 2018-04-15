using System;
using System.Collections.Generic;

namespace wmsMLC.General.BL
{
    public class TraceExtension
    {
        public const string ActivityTrackingSourcePropertyName = "_$ActivityTrackingSource";
        public const string ParentWfInstanceIdPropertyName = "_$ParentWfInstanceId";

        public TraceExtension()
        {
            Properties = new Dictionary<string, object>();
        }

        public bool NotUseActivityStackTrace { get; set; }
        public bool UsePersistAndLog { get; set; }

        public IDictionary<string, object> Properties { get; private set; }

        public T Get<T>(string name)
        {
            if (string.IsNullOrEmpty(name) || !Properties.ContainsKey(name))
                return default(T);

            try
            {
                return (T)SerializationHelper.ConvertToTrueType(Properties[name], typeof(T));
            }
            catch (Exception ex)
            {
                throw new DeveloperException(string.Format("Value of the '{0}' can not be converted to '{1}'.", name, typeof(T)), ex);
            }
        }

        public void Set<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            
            Properties[name] = value;
        }
    }
}
