using System;
using System.ServiceModel;
using System.Configuration;

namespace wmsMLC.General.Services.Service
{
    /// <summary>
    /// ������� ����� ��� ������� ���������� � ������� � ��������� �����������������
    /// </summary>
    public abstract class AppHostSvc : AppHostBase
    {
        #region .  Fields & Consts  .
        protected readonly SvcConfigBase Config;
        #endregion

        protected AppHostSvc(ServiceContext context) : base(context)
        {
            Config = new SvcConfigBase(context);

            ServiceName = context.Name;

//            SendTimeout = Settings.Default.SendTimeoutInMS;
//            ReceiveTimeout = Settings.Default.ReceiveTimeoutInMS;
        }

        #region .  Methods  .
        protected override void DoHostInternal(object context)
        {
            InitSettings();

            using (var host = CreateServiceHost())
            {
                // ���������
                host.Opened += HostOpened;
                host.Faulted += HostFaulted;

                try
                {
                    host.Open();
                }
                catch (Exception ex)
                {
                    Log.Error("������ ������� �������. " + ExceptionHelper.ExceptionToString(ex), ex);
                    throw;
                }

                // ����� � �� ���������
                if (host.State != CommunicationState.Opened)
                    throw new DeveloperException(Resources.StringResources.FailedToStartService);

                // ��������� � ��������
                Wait();
            }
        }

        /// <summary>
        /// ��������� (����� �������� � �������� ������, ��� �����, ��� � ������������)
        /// </summary>
        protected virtual void InitSettings()
        {
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        /// <summary>
        /// ��������� �����. �������� �������� ��� �������� ���� ������� � �����
        /// </summary>
        /// <returns></returns>
        protected abstract ServiceHost CreateServiceHost();

        protected virtual void HostOpened(object sender, EventArgs e)
        {
            Log.Debug(Resources.StringResources.ServiceIsRunned);
            Log.Debug("========== Config ==========");
            var host = ((ServiceHost)sender);
            Log.DebugFormat("Service name: '{0} (at {1})'", host.Description.Name, host.Description.Namespace);
            Log.DebugFormat("Service type: '{0}'", host.Description.ServiceType);
            Log.DebugFormat("Service endpoints:");
            for (int i = 0; i < host.Description.Endpoints.Count; i++)
            {
                var ep = host.Description.Endpoints[i];
                Log.DebugFormat("   {0}: {1} ({2})", i, ep.ListenUri, ep.Contract.ContractType);
            }
            Log.Debug("============================");
        }

        protected virtual void HostFaulted(object sender, EventArgs eventArgs)
        {
            Log.Debug("Host faulted");
        }

        protected Uri GetUri()
        {
            if (string.IsNullOrEmpty(Config.Http))
                throw new DeveloperException("Service url is empty in config");
            var url = Config.Http;
            // ���������, ����� ��������� ������ �� ��� "/"
            if (url[url.Length - 1] == '/')
                url = url.Substring(0, url.Length - 1);
            return new Uri(url);
        }

        #endregion
    }
}