using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Windows;
using Microsoft.AspNet.SignalR.Client;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Helpers
{
    public class SignalRHelper
    {
        public static void TryConnectToServer()
        {
            var querystringData = new Dictionary<string, string> {{"userName", GetCurrentUserLogin()}};
            var serverUrl = GetServerUrl();
            var hubConnection = new HubConnection(serverUrl, querystringData);
            var hubProxy = hubConnection.CreateHubProxy("DclHub");
            try
            {
                hubConnection.Start().Wait();
                WMSEnvironment.Instance.Set("HubConnection", hubConnection);

                hubProxy.On("WriteConsole", newMessage => WriteConsole(newMessage));
                hubProxy.On<BusinessEntityDescriptor>("ShowEntityCard", ShowEntityCard);
            }
            catch (Exception)
            {
                AppHelper.Log.Debug("Can't connect to SignalR Server at " + serverUrl);
            }
        }

        public static void DisconnectFromServer()
        {
            var hubConnection = WMSEnvironment.Instance.Get<HubConnection>("HubConnection");
                hubConnection.Stop();
        }

        private static string GetServerUrl()
        {
            var url = ConfigurationManager.AppSettings["WebclientUrl"] + "signalr/hubs";
            return url;
        }

        private static string GetCurrentUserLogin()
        {
            var userName = WMSEnvironment.Instance.AuthenticatedUser.GetSignature();

            // Get rid of domain
            var idx = userName.IndexOf("\\", StringComparison.Ordinal);
            if (idx > 0)
                userName = userName.Substring(idx + 1);

            return userName;
        }

        private static void WriteConsole(string newMessage)
        {
            AppHelper.Log.Debug(newMessage);
        }

        private static void ShowEntityCard(BusinessEntityDescriptor entityDescr)
        {
            Contract.Requires(entityDescr != null);
            Contract.Requires(!string.IsNullOrEmpty(entityDescr.PrimaryKeyDatatype));
            Contract.Requires(!string.IsNullOrEmpty(entityDescr.PrimaryKeyValue));
            Contract.Requires(!string.IsNullOrEmpty(entityDescr.EntityType));

            switch (entityDescr.EntityType.ToLower())
            {
                case "wmsiwb":
                    ShowEntityCardGeneric<IWB>(entityDescr);
                    break;
                case "wmsowb":
                    ShowEntityCardGeneric<OWB>(entityDescr);
                    break;
                default:
                    throw new SystemException("Editing of this EntityType is not supported yet: " + entityDescr.EntityType);
            }
        }

        private static void ShowEntityCardGeneric<T>(BusinessEntityDescriptor entityDescriptor)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            var viewModel = IoC.Instance.Resolve<IObjectViewModel<T>>();
            var mgr = IoC.Instance.Resolve<IBaseManager<T>>();

            object id;
            var pkType = entityDescriptor.PrimaryKeyDatatype.ToLower().Replace("system.", "");
            switch (pkType)
            {
                case "decimal":
                    id = Convert.ToDecimal(entityDescriptor.PrimaryKeyValue);
                    break;
                case "string":
                    id = entityDescriptor.PrimaryKeyValue;
                    break;
                default:
                    throw new SystemException("This pkType is not supported yet: " + entityDescriptor.PrimaryKeyDatatype);
            }
            
            var source = mgr.Get(id, GetModeEnum.Partial);
            if (source == null)
                throw new SystemException("source entity instance cannot be found. entityType: " + entityDescriptor.EntityType + ", pkValue: " + entityDescriptor.PrimaryKeyValue);
            
            viewModel.SetSource(source);
            viewService.Show(viewModel, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = false });
            
            DispatcherHelper.BeginInvoke(new Action(() => Application.Current.MainWindow.Activate()));
        }
    }

    public class BusinessEntityDescriptor
    {
        public string EntityType;
        public string PrimaryKeyDatatype;
        public string PrimaryKeyValue;
    }
}