using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class BpProcessTest : SiBaseTest
    {
        [Test]
        public void UowTest()
        {
            var cpvs = new List<CustomParamValue>
            {
                new CustomParamValue
                {
                    CPVID = -1,
                    CPV2Entity = "IWB",
                    CPVKey = "7570",
                    CustomParamCode = "IWBTIRConosament",
                    CPVValue = "Привет!"
                }
            };

            var productId = 495051;
            var packTE = "ABRTESTPACK1";
            var prdCount = 11;

            //using (var mgr = IoC.Instance.Resolve<BPProcessManagerOracle>())
            using (var mgr = IoC.Instance.Resolve<IBaseManager<BPProcess>>())
            {
                //((IBPProcessManager) mgr).SaveCpvs(cpvs, null);

                IUnitOfWork uow = null;
                try
                {
                    uow = UnitOfWorkHelper.GetUnit();
                    mgr.SetUnitOfWork(uow);
                    uow.BeginChanges();
                    ((BPProcessManager)mgr).PackProduct(productId, packTE, prdCount);
                    uow.RollbackChanges();
                    //uow.CommitChanges();
                }
                catch
                {
                    uow.RollbackChanges();
                    throw;
                }
            }
        }
    }
}
