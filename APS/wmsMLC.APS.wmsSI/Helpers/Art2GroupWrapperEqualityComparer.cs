using System.Collections.Generic;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.APS.wmsSI.Helpers
{
    public class Art2GroupWrapperEqualityComparer : IEqualityComparer<Art2GroupWrapper> 
    {
        public bool Equals(Art2GroupWrapper x, Art2GroupWrapper y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.ART2GROUPARTGROUPCODE.Equals(y.ART2GROUPARTGROUPCODE);
        }

        public int GetHashCode(Art2GroupWrapper obj)
        {
            if (ReferenceEquals(obj, null))
                return 0;

            var propertyValue = obj.ART2GROUPARTGROUPCODE;
            return propertyValue == null ? 0 : propertyValue.GetHashCode();
        }
    }
}
