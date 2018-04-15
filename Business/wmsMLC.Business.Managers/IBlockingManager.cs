using System;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public interface IBlockingManager
    {
        void Block(WMSBusinessObject obj, ProductBlocking blocking, string description);
        Type GetBlockingType();
        string GetNameLookupBlocking();
    }
}