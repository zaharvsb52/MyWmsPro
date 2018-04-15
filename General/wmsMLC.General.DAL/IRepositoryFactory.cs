namespace wmsMLC.General.DAL
{
    public interface IRepositoryFactory
    {
        T Get<T>() where T : class, IBaseRepository;
        T Get<T>(IUnitOfWork uow) where T : class, IBaseRepository;
        T Get<T>(IUnitOfWork uow, bool dispose) where T : class, IBaseRepository;
    }
}