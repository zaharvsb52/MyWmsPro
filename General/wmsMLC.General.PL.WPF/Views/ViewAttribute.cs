using System;

namespace wmsMLC.General.PL.WPF.Views
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ViewAttribute : Attribute
    {
        public Type ViewType { get; private set; }
        public object Context { get; private set; }

        public ViewAttribute(Type viewType, object context = null)
        {
            ViewType = viewType;
            Context = context;
        }
    }
}