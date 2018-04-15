using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.Tests.Functional.Entities
{
    //TODO: Перенести в TECH-тесты
    [TestFixture, Ignore("In progress")]
    public class UoWUsingTest
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
        }

        [Test]
        public void UoWCreatedOutsideManager()
        {
            var factory = IoC.Instance.Resolve<IUnitOfWorkFactory>();

            using (var uow = factory.Create())
            {
                uow.BeginChanges();

                var mgrAreaType = IoC.Instance.Resolve<IBaseManager<AreaType>>();
                var mgrMandant = new MandantManager();

                mgrAreaType.SetUnitOfWork(uow, false);
                mgrMandant.SetUnitOfWork(uow, false);

                mgrAreaType.GetAll();
                mgrMandant.GetAll();

                uow.RollbackChanges();
            }
        }

        [Test]
        public void UoWCreateInEachManager()
        {
            var mgrAreaType = IoC.Instance.Resolve<IBaseManager<AreaType>>();
            var mgrMandant = new MandantManager();

            mgrAreaType.GetAll();
            mgrMandant.GetAll();
        }

        [Test]
        public void MoreThanOneManagerCanWorkWithOneUoW()
        {
            using (var uow = UnitOfWorkHelper.GetUnit(false))
            {
                uow.BeginChanges();

                using (var mgr = IoC.Instance.Resolve<IBaseManager<TEType>>())
                {
                    mgr.SetUnitOfWork(uow);
                    var items = mgr.GetAll();
                }

                using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                {
                    mgr.SetUnitOfWork(uow);
                    var items = mgr.GetAll();
                }

                uow.RollbackChanges();
            }
        }
    }
}