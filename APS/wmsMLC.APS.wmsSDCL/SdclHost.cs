using System;
using System.Configuration;
using System.ServiceModel;
using wmsMLC.Business;
using wmsMLC.General;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsSDCL
{
    class SdclHost : AppHostSvc
    {
        public SdclHost(ServiceContext context) : base(context) { }

        protected override void InitSettings()
        {
            base.InitSettings();

            // выставим среду, если указана
            var env = Context.Get(ConfigBase.EnvironmentParam);

            ConfigurationManager.AppSettings["BLToolkit.DefaultConfiguration"] = string.IsNullOrEmpty(env) ? "DEV" : env;

            // TODO: убрать использование HandlerId
            SDCL.HandlerId = Context.Get(ConfigBase.HandlerParam);

            // явно инициализируем Oracle - чтобы не было ни каких накладок
            BLHelper.InitBL(dalType: DALType.Oracle);

            // аутентифицируемся
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate(ConfigurationManager.AppSettings["Login"], ConfigurationManager.AppSettings["Password"]);

            //Загрузим начальные кэши
            BLHelper.FillInitialCaches();
        }

        protected override ServiceHost CreateServiceHost()
        {
            return new ServiceHost(typeof(SDCL), new Uri(Config.Endpoint));
        }
    }
}
