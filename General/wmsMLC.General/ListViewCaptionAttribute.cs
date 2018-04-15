using System;

namespace wmsMLC.General
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class ListViewCaptionAttribute : Attribute
    {
        public ListViewCaptionAttribute(string caption)
        {
            Caption = caption;
        }

        public string Caption { get; private set; }
    }
}
