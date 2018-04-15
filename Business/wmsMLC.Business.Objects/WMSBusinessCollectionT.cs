using System;
using System.Collections.Generic;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class WMSBusinessCollection<T> : EditableBusinessObjectCollection<T>, ICloneable
    {
        public WMSBusinessCollection()
        {
            
        }

        public WMSBusinessCollection(IEnumerable<T> collection)
            : base(collection)
        {
            
        }

        public virtual object Clone()
        {
            var clone = Activator.CreateInstance(GetType()) as WMSBusinessCollection<T>;
            foreach (var item in clone)
            {
                if (item == null || item.GetType().IsPrimitive || item.GetType().IsValueType)
                    clone.Add(item);
                else
                {
                    var cloneableItem = item as ICloneable;
                    if (cloneableItem != null)
                        clone.Add((T)cloneableItem.Clone());
                    else
                        throw new NotImplementedException();
                }
            }
            return clone;
        }

        public override string ToString()
        {
            return typeof(T).GetDisplayName();
        }
    }
}