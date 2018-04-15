using System;
using System.Diagnostics.Contracts;

namespace MLC.SvcClient
{
    /// <summary>
    /// Параметр серверной транзакции
    /// </summary>
    public class Parameter
    {
        public Parameter(string name, object value)
            : this(name, value, value == null ? typeof(object) : value.GetType())
        {

        }

        public Parameter(string name, object value, Type type)
        {
            Contract.Requires(name != null);

            Name = name;
            Value = value;
            Type = type;
        }

        public string Name { get; private set; }
        public object Value { get; private set; }
        public Type Type { get; private set; }
    }
}