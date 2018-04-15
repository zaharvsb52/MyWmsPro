using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Threading;
using wmsMLC.Business;
using wmsMLC.Business.General;
using wmsMLC.Business.Managers.Imports;
using wmsMLC.General;
using wmsMLC.General.Services.Service;
using wmsMLC.General.Types;

namespace wmsMLC.APS.wmsSI
{
    public class IntegrationServiceHost : AppHostSvc
    {
        private const int MinTimerPeriod = 1000;

        private Timer _periodTimer;

        //Параметры
        private string _apiUri;
        private string _inbound;
        private string _imported;
        private long _timerPeriod;

        //Параметр насосной станции
        private readonly string _linkById;

        public IntegrationServiceHost(ServiceContext context) : base(context)
        {
            _linkById = Guid.NewGuid().ToString();
        }

        protected override void InitSettings()
        {
            base.InitSettings();

            // выставим среду, если указана
            var env = Config.Environment;
            ConfigurationManager.AppSettings["BLToolkit.DefaultConfiguration"] = string.IsNullOrEmpty(env) ? "DEV" : env;

            // явно инициализируем Oracle - чтобы не было ни каких накладок
            BLHelper.InitBL(dalType: DALType.Oracle);

            // аутентифицируемся
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate(ConfigurationManager.AppSettings["Login"], ConfigurationManager.AppSettings["Password"]);

            //Загрузим начальные кэши
            BLHelper.FillInitialCaches();

            //Выставляем значения параметров
            //_apiUri = Context.Get("apiendpoint"); 
            _apiUri = ConfigurationManager.AppSettings["APIEndPoint"]; //Значение _apiUri считываем из app.config
            _inbound = Context.Get("smbinbound");
            _imported = Context.Get("smbimported");
            _timerPeriod = Context.Get("watchdog").To(MinTimerPeriod) * 1000;
            if (_timerPeriod < MinTimerPeriod)
                _timerPeriod = MinTimerPeriod;

            //Запускаем таймер
            FreeTimer();
            _periodTimer = new Timer(DeliveryTime);
            _periodTimer.Change(0, _timerPeriod); // стартуем сразу, а потом через период
        }

        protected override ServiceHost CreateServiceHost()
        {
            return new ServiceHost(typeof(IntegrationService), GetUri());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_periodTimer != null)
                    FreeTimer();
            }

            base.Dispose(disposing);

            Log.DebugFormat("Service was uninitialized");
        }

        private void DeliveryTime(object state)
        {
            if (_periodTimer == null)
                return;

            try
            {
                _periodTimer.Change(-1, -1);
                var pumper = new FilePumper(_linkById);

                try
                {
                    pumper.Initialize(_inbound, _imported, new[] { "*.*" });
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    //LogClient.Logger.OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { string.Empty }, "DELIVERY_TIME_PUMPER", LinkById);
                }
                //получим все файлы во входящей папке
                var files = pumper.GetFileList();
                if (files.Length > 0)
                {
                    Log.Debug(string.Format("Получено(а) {0} телеграмм(а)", files.Length));
                    foreach (string file in pumper.GetFileList())
                    {
                        //LogClient.Logger.OnEventErrorHandler(null, ErrorType.INFO, "Импорт данных из файла {0}", new string[] { file }, "DELIVERY_TIME", LinkById);
                        Log.Debug(string.Format("Импорт данных из файла {0}", file));
                        var xml = Encoding.UTF8.GetString(pumper.GetFile(file, true));
                        var result = ValidateAndImport(xml);
                        var outFile = Path.Combine(_imported, string.Format("{0}_{1}_Response.xml", Path.GetFileNameWithoutExtension(file), result.Id));
                        File.WriteAllText(outFile, result.DumpToXML());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex); 
                //LogClient.Logger.OnEventErrorHandler(ex, ErrorType.INFO, "EUnknown", new[] { "" }, "SI_SERVICE_MANAGER_PERIODTIMER", LinkById);
            }
            finally
            {
                _periodTimer.Change(_timerPeriod, _timerPeriod);
            }
        }

        private ImportObject ValidateAndImport(string xmlString)
        {
            var mgr = new ImportManager();
            mgr.SetApiUri(_apiUri);
            return mgr.ProcessImport(xmlString);
        }

        private void FreeTimer()
        {
            if (_periodTimer == null)
                return;

            _periodTimer.Change(-1, -1);
            _periodTimer.Dispose();
            _periodTimer = null;
        }
    }
}