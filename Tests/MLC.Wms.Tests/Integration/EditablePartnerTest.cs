using NUnit.Framework;
using wmsMLC.Business.Managers;

namespace wmsMLC.Business.Tests
{
    [TestFixture]
    public class EditablePartnerTest
    {
        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL();
        }
        
        [Test]
        public void AcceptChangesTest()
        {
            var mgr = new PartnerManager();
            var items = mgr.GetFiltered(null);
            foreach (var partner in items)
            {
                //partner.BeginMapping(null);
                //partner.EndMapping(null);
                //partner.BeginEdit();
                partner.SetProperty("Partner2Mandant", true);
                partner.SetProperty("Partner2Mandant", false);
                partner.AcceptChanges();
                //partner.EndEdit();
            }
        }

        [Test]
        public void RejectChangesTest()
        {
            var mgr = new PartnerManager();
            var items = mgr.GetFiltered(null);
            foreach (var partner in items)
            {
                //partner.BeginMapping(null);
                //partner.EndMapping(null);
                //partner.BeginEdit();
                partner.SetProperty("Partner2Mandant", true);
                partner.SetProperty("Partner2Mandant", false);
                partner.RejectChanges();
                //partner.EndEdit();
            }
        }        
    }
}
