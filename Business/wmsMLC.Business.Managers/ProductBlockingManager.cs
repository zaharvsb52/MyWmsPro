using System;
using System.Collections.Generic;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер блокировок.
    /// </summary>
    public class ProductBlockingManager : WMSBusinessObjectManager<ProductBlocking, string>, IProductBlockingManager
    {
        public IEnumerable<ProductBlocking> GetBlockingForProduct()
        {
            using (var repo = GetRepository<IProductBlockingRepository>())
                return repo.GetBlockingForProduct();
        }

        public IEnumerable<ProductBlocking> GetBlockingForTE()
        {
            using (var repo = GetRepository<IProductBlockingRepository>())
                return repo.GetBlockingForTE();
        }

        public IEnumerable<ProductBlocking> GetBlockingForPlace()
        {
            using (var repo = GetRepository<IProductBlockingRepository>())
                return repo.GetBlockingForPlace();
        }
    }

    public class ProductManager : WMSBusinessObjectManager<Product, decimal>, IProductManager
    {
        public IEnumerable<Product> GetByTECode(string teCode, bool byNested = false, GetModeEnum mode = GetModeEnum.Partial)
        {
            if (string.IsNullOrEmpty(teCode))
                throw new ArgumentNullException("teCode");

            var filter = !byNested ? string.Format("TECODE_R = '{0}'", teCode) : string.Format("TECODE_R in (select wmste.tecode from wmste start with (TECarrierStreakCode = '{0}' or tecode='{0}') connect by prior TECode = TECarrierStreakCode) ", teCode);
            return GetFiltered(filter, mode);
        }
    }

    public interface IProductManager : IBaseManager<Product, decimal>
    {
        IEnumerable<Product> GetByTECode(string teCode, bool byNested = false, GetModeEnum mode = GetModeEnum.Partial);
    }
}
