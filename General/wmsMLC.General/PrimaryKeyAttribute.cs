using System;

namespace wmsMLC.General
{
    /// <summary>
    /// Аттрибут позволяет указать свойство объекта, содержащее уникальный первичный ключ
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class PrimaryKeyAttribute : Attribute
    {
    }
}
