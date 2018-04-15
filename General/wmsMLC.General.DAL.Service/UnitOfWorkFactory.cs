namespace wmsMLC.General.DAL.Service
{
    public sealed class UnitOfWorkFactory : BaseUnitOfWorkFactory<UnitOfWork>
    {
        protected override UnitOfWork Create_Internal(UnitOfWorkContext context)
        {
            if (context == null)
                return new UnitOfWork(); // INFO: ВНИМАНИЕ!!! телеграмма не будет подписана
            return new UnitOfWork(context);
        }
    }
}