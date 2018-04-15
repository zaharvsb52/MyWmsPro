using System;
using System.Collections.Generic;

namespace wmsMLC.General.BL
{
    public class ActivityStackTrace : IDisposable
    {
        private List<InfoOfActivity> _activities;

        public List<InfoOfActivity> Activities
        {
            get { return _activities ?? (_activities = new List<InfoOfActivity>()); }
        }

        public InfoOfActivity FaultHandler { get; set; }
        public InfoOfActivity FaultSource { get; set; }
        public Exception Fault { get; set; }

        public void ClearFault()
        {
            FaultHandler = null;
            FaultSource = null;
            Fault = null;
        }

        public void Clear()
        {
            ClearFault();
            _activities = null;
        }

        public void Dispose()
        {
            Clear();
        }
    }

    public sealed class InfoOfActivity : IComparable
    {
        public InfoOfActivity(string name, string id, string instanceId, string typeName)
        {
            Name = name;
            Id = id;
            InstanceId = instanceId;
            TypeName = typeName;
        }

        public string Name { get; private set; }
        public string Id { get; private set; }
        public string InstanceId { get; private set; }
        public string TypeName { get; private set; }
        public string Info { get; set; }

        public override string ToString()
        {
            var info = string.IsNullOrEmpty(Info) ? null : string.Format(", {0}", Info);
            return string.Format("Name='{0}'{1}, TypeName='{2}', ActivityId='{3}', ActivityInstanceId='{4}'", Name, info, TypeName, Id, InstanceId);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) 
                return 1;
            var other = obj as InfoOfActivity;
            if (other == null)
                throw new ArgumentException("Object is not a InfoOfActivity");

            return String.Compare(GetString(), other.GetString(), StringComparison.Ordinal);
        }

        private string GetString()
        {
            return string.Format("{0}'{1}'{2}'{3}", Name, TypeName, Id, InstanceId);
        }
    }
}
