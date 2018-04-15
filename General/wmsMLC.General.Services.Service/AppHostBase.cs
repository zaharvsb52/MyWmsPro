using System;
using System.ServiceProcess;
using System.Threading;
using log4net;

namespace wmsMLC.General.Services.Service
{
    /// <summary>
    /// Базовый класс для запуска приложений
    /// </summary>
    public abstract class AppHostBase : ServiceBase, IAppHost
    {
        #region .  Fields  .
        protected readonly ILog Log;
        private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

        private Thread _thread;
        #endregion

        protected AppHostBase(ServiceContext context)
        {
            Log = LogManager.GetLogger(GetType());

            Context = context;
        }

        public ServiceContext Context { get; set; }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            try
            {
                _thread = new Thread(DoHost) { Priority = ThreadPriority.Highest };
                _thread.Start(Context);
            }
            catch (Exception ex)
            {
                Log.Error("Unhandled Host exception. " + ExceptionHelper.ExceptionToString(ex), ex);
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            Resume();
            _thread.Join();
        }

//        public void Start(string[] args)
//        {
//            try
//            {
//                _thread = new Thread(DoHost) { Priority = ThreadPriority.Highest };
//                _thread.Start(Context);
//            }
//            catch (Exception ex)
//            {
//                Log.Error("Unhandled Host exception. " + ExceptionPolicy.ExceptionToString(ex), ex);
//            }
//        }
//
//        public void Stop()
//        {
//            Resume();
//            _thread.Join();
//        }

        private void DoHost(object context)
        {
            try
            {
                DoHostInternal(context);
            }
            catch (Exception ex)
            {
                Log.Error("Host exception: " + ExceptionHelper.ExceptionToString(ex), ex);
            }
        }

        protected void Wait()
        {
            // подвисаем в ожидании
            WaitHandle.WaitOne();
        }

        protected void Resume()
        {
            // подвисаем в ожидании
            WaitHandle.Set();
        }

        protected abstract void DoHostInternal(object context);
    }
}