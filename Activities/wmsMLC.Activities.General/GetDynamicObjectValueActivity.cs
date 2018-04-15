using System;
using System.Activities;
using System.Collections.Generic;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class GetDynamicObjectValueActivity : NativeActivity<object>
    {
        [Required]
        public InArgument<dynamic> InputObject { get; set; }

        [Required]
        public InArgument<string> PropertyName { get; set; }

        public GetDynamicObjectValueActivity()
        {
            this.DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var dyn = InputObject.Get(context) as IDictionary<string, object>;
            var prop = PropertyName.Get(context);
            var result = dyn[prop];
            context.SetValue(Result, result);
            Console.WriteLine("{0} = {1}", prop, result);
        }
    }
}
