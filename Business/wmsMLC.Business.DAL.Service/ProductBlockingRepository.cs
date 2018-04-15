using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;

namespace wmsMLC.Business.DAL.Service
{
    /// <summary>
    /// Репозиторий блокировок
    /// </summary>
    public abstract class ProductBlockingRepository : BaseRepository<ProductBlocking, string>, IProductBlockingRepository
    {
        public IEnumerable<ProductBlocking> GetBlockingForProduct()
        {
            return GetFiltered(IsTrueFilterString(ProductBlocking.BlockingForProductPropertyName), null);
        }

        public IEnumerable<ProductBlocking> GetBlockingForTE()
        {
            return GetFiltered(IsTrueFilterString(ProductBlocking.BlockingForTEPropertyName), null);
        }

        public IEnumerable<ProductBlocking> GetBlockingForPlace()
        {
            return GetFiltered(IsTrueFilterString(ProductBlocking.BlockingForPlacePropertyName), null);
        }

        private static string IsTrueFilterString(string propertyName)
        {
            return string.Format("({0}=1)", propertyName);
        }
    }
}
