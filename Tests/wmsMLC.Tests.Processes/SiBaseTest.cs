using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.General;

namespace wmsMLC.Tests.Processes
{
    public abstract class SiBaseTest
    {
        private static bool _initialized;

        [TestFixtureSetUp]
        public void Setup()
        {
            if (_initialized)
                return;

            _initialized = true;
            BLHelper.InitBL(dalType: DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
        }

        [Test]
        public void Test()
        {
            DoTest();
        }

        public virtual void DoTest()
        {
        }
    }
}
