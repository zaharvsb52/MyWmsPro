using BLToolkit.Reflection;

namespace wmsMLC.General.DAL.Service
{
    public class RepositoryFactory : BaseRepositoryFactory
    {
        public override T Get<T>(IUnitOfWork uow, bool dispose)
        {
            var repoType = GetRepoType<T>();
            if (repoType == null)
                throw new DeveloperException("Can't find repository type for object " + typeof(T));

            // NOTE: пока никак не используем UnitOfWork в сервисах
            // HACK: чтобы не изобретать велосипед с кэшированием используем стандартный механизм от BLToolkit
            var repo = (T)TypeAccessor.CreateInstance(repoType);
            var br = repo as BaseRepository;
            if (br != null)
                br.SetUnitOfWork(uow, dispose);

            return repo;
        }
    }
}