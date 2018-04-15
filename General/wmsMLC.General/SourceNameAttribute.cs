using System;

namespace wmsMLC.General
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class SourceNameAttribute : Attribute
    {
        public SourceNameAttribute(string sourceName)
        {
            this.SourceName = sourceName;
        }

        public string SourceName { get; private set; }
    }
}