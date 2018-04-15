using wmsMLC.General.DAL;

namespace wmsMLC.General.BL
{
    public static class UnitOfWorkHelper
    {
        private static IUnitOfWorkFactory GetFactory()
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            if (uowFactory == null)
                throw new DeveloperException("IUnitOfWorkFactory is not registered");
            return uowFactory;
        }

        public static IUnitOfWork GetUnit(bool dispose = false, int? timeOut = null)
        {
            var factory = GetFactory();
            return factory.Create(dispose, timeOut);
        }

        public static IUnitOfWork GetUnit(UnitOfWorkContext context)
        {
            var factory = GetFactory();
            return factory.Create(context);
        }
    }
}
