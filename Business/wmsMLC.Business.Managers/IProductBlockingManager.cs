using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IProductBlockingManager : IBaseManager<ProductBlocking, string>
    {
        IEnumerable<ProductBlocking> GetBlockingForProduct();
        IEnumerable<ProductBlocking> GetBlockingForTE();
        IEnumerable<ProductBlocking> GetBlockingForPlace();
    }
}
