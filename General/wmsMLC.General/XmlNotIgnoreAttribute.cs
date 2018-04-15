using System;

namespace wmsMLC.General
{
    /// <summary>
    /// Атрибут разрешающий CUD-операции над сущностью, в случае, если сущность с данным атрибутом является вложенной.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class XmlNotIgnoreAttribute : Attribute
    {
    }
}
