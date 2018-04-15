using System;

namespace wmsMLC.General.PL.Model
{
    public sealed class NameValueObject : ICloneable
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public object Clone()
        {
            return this.Clone(GetType(), false);
        }
    }
}
