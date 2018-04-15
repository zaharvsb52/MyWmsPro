using System;
using System.Activities;
using System.Reflection;
using wmsMLC.General;

namespace wmsMLC.Activities.General.Helpers
{
    public class ExecuteProcedureParamValues : ICloneable
    {
        public ExecuteProcedureParamValues() { }
        public ExecuteProcedureParamValues(ParameterInfo p)
        {
            Name = p.Name;
            DisplayName = p.Name;
            TypeName = p.ParameterType;
            Direction = p.IsOut
                            ? ArgumentDirection.Out
                            : p.IsRetval
                                  ? ArgumentDirection.InOut
                                  : ArgumentDirection.In;
            Value = null;
        }

        //[ReadOnly(true)]
        //[DisplayName(@"Тип")]
        public Type TypeName { get; set; }

        //[ReadOnly(true)]
        //[DisplayName(@"Имя")]
        public string DisplayName { get; set; }

        //[ReadOnly(true)]
        //[DisplayName(@"Направление")]
        public ArgumentDirection Direction { get; set; }

        //[DisplayName(@"Значение")]
        public object Value { get; set; }

        //[Bindable(false)]
        //[Browsable(false)]
        public string Name { get; set; }

        /// <summary>
        /// Path для создания Binding.
        /// </summary>
        public string BindingPath { get; set; }

        public virtual object Clone()
        {
            return this.Clone(GetType(), false);
        }
    }
}
