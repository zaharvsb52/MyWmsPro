using System.Activities;
using System.ComponentModel;
using System.Linq;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class GetValueActivity<T> : NativeActivity<object>
    {
        [Required]
        public InArgument<T> Source { get; set; }
        
        [Required]
        public InArgument<string> PropertyName { get; set; }

        public GetValueActivity()
        {
            this.DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var source = Source.Get(context);
            var propertyName = PropertyName.Get(context);
            var properties = TypeDescriptor.GetProperties(source);
            var property = properties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.Equals(propertyName, System.StringComparison.InvariantCultureIgnoreCase));
            if (property != null)
                context.SetValue(Result, property.GetValue(source));
        }
    }
}
