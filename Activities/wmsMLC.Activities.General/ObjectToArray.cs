using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class ObjectToArray<T> : NativeActivity<T[]>
    {
        [Required]
        public InArgument<object> Input { get; set; }

        public ObjectToArray()
        {
            this.DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var res = (IEnumerable)Input.Get(context);
            var col = res.Cast<T>().ToArray();
            context.SetValue(Result, col);
        }
    }

    /*
    public class Cast<T> : NativeActivity<T>
    {
        [Required]
        public InArgument<object> Input { get; set; }

//        [Required]
//        public OutArgument<object> Output { get; set; }

        public Cast()
        {
            this.DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var source = Input.Get(context);
            //var dest = Output.Get(context);
            T dest;

            var enumerableSource = source as IEnumerable;
            if (enumerableSource != null)
            {
                var isEnumerable = typeof (IEnumerable<T>).IsAssignableFrom(typeof (T));
                if (!isEnumerable)
                    throw new Exception();

                var itemType = typeof(T).GetElementType();

                dest = enumerableSource.Cast
            }
            var col = res.Cast<T>().ToArray();
            context.SetValue(Result, col);
        }
    }
    */
}
