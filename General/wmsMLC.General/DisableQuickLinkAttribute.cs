using System;

namespace wmsMLC.General
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class DisableQuickLinkAttribute : Attribute
    {
    }
}