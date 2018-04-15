using System.Collections.Generic;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.APS.wmsSI.Helpers
{
    public class TransitDataWrapperEqualityComparer : IEqualityComparer<TransitDataWrapper> 
    {
        public bool Equals(TransitDataWrapper x, TransitDataWrapper y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.TRANSITNAME.Equals(y.TRANSITNAME);
        }

        public int GetHashCode(TransitDataWrapper obj)
        {
            if (ReferenceEquals(obj, null))
                return 0;

            var propertyValue = obj.TRANSITNAME;
            return propertyValue == null ? 0 : propertyValue.GetHashCode();
        }
    }
}
