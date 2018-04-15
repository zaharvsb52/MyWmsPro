using System;
using System.Collections.Generic;

namespace wmsMLC.General.BL
{
    public class CustomPropertyCollection : Dictionary<string, CustomProperty> //List<CustomProperty>
    {
        public CustomPropertyCollection() : base(20, StringComparer.OrdinalIgnoreCase) { }

        public new CustomProperty this[string key]
        {
            get
            {
                // переопределяем поведение Dictionary (убираем KeyNotFoundException)
                CustomProperty res;
                if (base.TryGetValue(key, out res))
                    return res;

                return null;
            }
            set
            {
                base[key] = value;
            }
        }
    }
}