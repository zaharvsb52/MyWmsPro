using BLToolkit.DataAccess;

namespace wmsMLC.General.DAL.Oracle
{
    public sealed class RepositoryFactory : BaseRepositoryFactory
    {
        public override T Get<T>(IUnitOfWork uow, bool dispose)
        {
            var repoType = GetRepoType<T>();
            if (repoType == null)
                throw new DeveloperException("Can't find repository type for object " + typeof(T));

            if (uow == null)
                return DataAccessor.CreateInstance(repoType) as T;

            var typedUoW = uow as UnitOfWork;
            if (typedUoW == null)
                throw new DeveloperException("Tring to use non Oracle UnitOfWork with Oracle RepositoryFactory.");

            DataAccessor da;

            // если репозиторий умеет работать с uow, то подсовываем ему наш
            if (typeof(IUnitOfWorkUser).IsAssignableFrom(repoType))
            {
                da = DataAccessor.CreateInstance(repoType);
                ((IUnitOfWorkUser)da).SetUnitOfWork(uow, dispose);
            }
            else
                da = DataAccessor.CreateInstance(repoType, typedUoW.DbManager, dispose);

            return da as T;
        }
    }
}