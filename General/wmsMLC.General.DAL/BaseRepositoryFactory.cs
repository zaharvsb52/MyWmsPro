using System;
using System.Collections.Concurrent;

namespace wmsMLC.General.DAL
{
    public abstract class BaseRepositoryFactory : IRepositoryFactory
    {
        private static readonly ConcurrentDictionary<Type, Type> _repoTypeCache = new ConcurrentDictionary<Type, Type>();

        public T Get<T>() where T : class, IBaseRepository
        {
            return Get<T>(null);
        }

        public T Get<T>(IUnitOfWork uow) where T : class, IBaseRepository
        {
            return Get<T>(uow, false);
        }

        public abstract T Get<T>(IUnitOfWork uow, bool dispose) where T : class, IBaseRepository;

        protected static Type GetRepoType<T>()
        {
            var repoType = typeof(T);
            if (!repoType.IsInterface)
                return repoType;

            // если от нас хотят интерфейс - значит нужно найти его реализатор
            // операция достаточно ресурсоемкая - кэшируем результаты
            return _repoTypeCache.GetOrAdd(repoType, t =>
            {
                // если от нас хотят интерфейс - значит нужно найти его реализатор
                if (repoType.IsInterface)
                    repoType = IoC.Instance.ResolveType<T>();
                return repoType;
            });
        }
    }
}