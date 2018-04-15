using System;

namespace wmsMLC.General
{
    /// <summary>
    /// Аттрибут позволяет указать свойство объекта, содержащее datetimeLongFormat
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class DbTypeAttribute : Attribute
    {
        public DbTypeCustom DBTypeCustom { get; }

        public DbTypeAttribute(DbTypeCustom dbType)
        {
            DBTypeCustom = dbType;
        }

        public enum DbTypeCustom
        {
            TimeStamp
        }
    }


}