using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IProductBlockingRepository : IRepository<ProductBlocking, string>
    {
        IEnumerable<ProductBlocking> GetBlockingForProduct();
        IEnumerable<ProductBlocking> GetBlockingForTE();
        IEnumerable<ProductBlocking> GetBlockingForPlace();
    }
}
