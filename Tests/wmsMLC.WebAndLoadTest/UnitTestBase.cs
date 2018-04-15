using System;
using System.Collections;
using log4net.Config;
using wmsMLC.Business;
using wmsMLC.General;

namespace wmsMLC.WebAndLoadTest
{
    public abstract class UnitTestBase : MarshalByRefObject, IUnitTest
    {
        public virtual void Initialize(IDictionary parameters)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            XmlConfigurator.Configure();
            ExceptionPolicy.Instance.Init();
            BLHelper.InitBL(dalType: DALType.Service);
            BLHelper.RegisterServiceClient("Auto", ClientTypeCode.DCL, null);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
            BLHelper.FillInitialCaches();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //throw new OperationCanceledException(((Exception)e.ExceptionObject).Message);
        }

        public virtual void Terminate()
        {
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.LogOff();
        }

        public virtual void Run()
        {
        }
    }
}
