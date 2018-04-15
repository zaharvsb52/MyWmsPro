using System.Linq;
using System.Linq.Dynamic;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class DynamicQueryTest : SiBaseTest
    {
        public override void DoTest()
        {
            string s = "PARTNERCODE == \"IDM\"";
            var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>();
            var objList = mgr.GetAll();
            var q = objList.AsQueryable().Where(s);
            foreach (var i in q)
            {
                var a = i.GetKey();
            }
        }
    }
}
