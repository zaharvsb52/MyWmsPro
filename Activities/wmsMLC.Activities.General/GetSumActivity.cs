using System;
using System.Activities;
using System.ComponentModel;
using System.Linq;
using wmsMLC.General;

namespace wmsMLC.Activities.General
{
    public class GetSumActivity<T> : NativeActivity
    {
        public InArgument<T[]> SourceList { get; set; }
        public InArgument<string> PropertyName { get; set; }
        public OutArgument<int> Result { get; set; }

        public GetSumActivity()
        {
            this.DisplayName = @"Сумма целочисленных значений в списке";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var source = SourceList.Get(context);
            var propertyName = PropertyName.Get(context);
            var properties = TypeDescriptor.GetProperties(typeof (T));
            var property = properties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.Equals(propertyName, System.StringComparison.InvariantCultureIgnoreCase));
            if (property == null) throw new DeveloperException("Задан не верный параметр {0}", propertyName);
            var sum = source.Sum(o => Convert.ToInt32(property.GetValue(o)));
            context.SetValue(Result, sum);
        }
    }
}
