namespace wmsMLC.General.DAL
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create(bool isShortSession = true, int? timeOut = null);
        IUnitOfWork Create(UnitOfWorkContext context);
        void Rollback();
    }
}