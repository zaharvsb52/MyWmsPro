using System;

namespace wmsMLC.General
{
    /// <summary>
    /// Аттрибут говорит, что при частичном получении сущности (только шапки) данный элемент должен быть включен в схему
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class IncludeInPartiallyGetAttribute : Attribute
    {
    }
}