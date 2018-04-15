using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using wmsMLC.Business.DAL.WebAPI;
using wmsMLC.General;
using wmsMLC.General.PL.Properties;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Enums;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.RCL.Resources;
using ExceptionResources = wmsMLC.General.Resources.ExceptionResources;

namespace wmsMLC.RCL.Main.Views
{
    public partial class ErrorBox : INotifyPropertyChanged
    {
        #region .  Static & Constants  .
        private const int MaxLenght = 100;
        private const string ErrorMessageSubjectTemplate = "{0} application error";
        private readonly ILog _log = LogManager.GetLogger(typeof(ErrorBox));
        private static readonly Dictionary<string, string> StaticAdditionalParams = new Dictionary<string, string>();
        #endregion .  Static & Constants  .

        #region .  Fields  .
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion .  Fields  .

        private ErrorBox()
        {
            ErrorSound = SystemSoundEnum.Hand;
            InitializeComponent();
            HelpServiceMail = Properties.Settings.Default.HelpServiceMail;
            IsBtnSandMailEnable = true;
            LayoutRoot.DataContext = this;
            SendMailCommand = new DelegateCustomCommand(this, OnSendMail, CanSendMail);
            CloseCommand = new DelegateCustomCommand(this, OnFormClose, CanFormClose);
#if DEBUG
            Topmost = false;
#endif
            Loaded += OnLoaded;
        }

        #region .  Properties  .
        private string _errorCaption;
        public string ErrorCaption
        {
            get { return _errorCaption; }
            set
            {
                if (_errorCaption == value)
                    return;
                _errorCaption = value;
                OnPropertyChanged("ErrorCaption");
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage == value)
                    return;
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public string HelpServiceMail { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public string[] Attachments { get; set; }
        public Dictionary<string, string> CurrentAdditionalParams { get; set; }
        public SystemSoundEnum ErrorSound { get; set; }
        public bool IsBtnSandMailEnable { get; private set; }
        public ICommand SendMailCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        #endregion .  Properties  .

        #region . Methods .
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            if (ErrorSound != SystemSoundEnum.None)
                new SoundHelper().PlaySystemSound(ErrorSound);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            KeyHelper.ViewPreviewKeyDown(this, e);
            base.OnPreviewKeyDown(e);
        }

        protected override void OnWindowClose(KeyEventArgs e)
        {
            if (e.Handled || !CanFormClose())
                return;

            switch (e.Key)
            {
                //case Key.Enter:
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    return;
            }
        }

        private bool CanSendMail()
        {
            return IsBtnSandMailEnable && !IsBusy;
        }

        private async void OnSendMail()
        {
            if (!CanSendMail())
                return;

            try
            {
                IsBusy = true;
                await SendMailAsync();
                Close();
            }
            catch (Exception ex)
            {
                Close();
                var mailex = new OperationException(BugReportResources.UnableSendEmailToSupport, ex);
                _log.Debug(mailex);
                ShowError(message: null, ex: mailex, additionalParams: null, attachments: null,
                    isBtnSandMailEnable: false);
            }
            finally
            {
                IsBtnSandMailEnable = false;
                IsBusy = false;
            }
        }

        private Task SendMailAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var sendMailImpl = new SendMailImpl();
                var to = HelpServiceMail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                var header = string.Format("{0}{1}{1}", ErrorMessage, Environment.NewLine);
                var mailinfo = CreateMail(to, string.Format(ErrorMessageSubjectTemplate, AssemblyAttributeAccessors.AssemblyProduct),
                    header, Message, Exception, CurrentAdditionalParams, Attachments);
                sendMailImpl.SendMail(mailinfo);
            });
        }

        private bool CanFormClose()
        {
            return !IsBusy;
        }

        private void OnFormClose()
        {
            if (CanFormClose())
                Close();
        }

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion . Methods .

        #region .  Static methods  .
        internal static void ShowError(string message, Exception ex, bool isBtnSandMailEnable = true)
        {
            ShowError(message, ex, null, null, isBtnSandMailEnable);
        }

        internal static void ShowError(string message, Exception ex, string[] attachments, Dictionary<string, string> additionalParams, bool isBtnSandMailEnable = true, string mandantCode = null)
        {
            // первая ошибка содержит описание
            var trueEx = ExceptionHelper.GetFirstMeaningException(ex);
            var exMessage = ExceptionHelper.GetErrorMessage(trueEx);

            // пробуем вытащить пояснение пользователю: оно может быть во вложенной ошибке
            var exReason = trueEx.InnerException is ICustomException
                ? string.Format(ExceptionResources.ErrorReason, ExceptionHelper.GetErrorMessage(trueEx.InnerException))
                : string.Empty;

            // собираем подробную информацию
            var exDetails = ExceptionHelper.ExceptionToString(ex);
            var totalDescription = string.Format("{0}{1}{1}{2}", exReason, Environment.NewLine, exDetails).Trim();

            DispatcherHelper.Invoke(
                new Action(() =>
                {
                    var wind = new ErrorBox
                    {
                        Message = message,
                        Exception = ex,
                        Attachments = attachments,
                        CurrentAdditionalParams = additionalParams,
                        ErrorCaption = GetStarMessage(exMessage) ?? string.Empty,
                        ErrorMessage = totalDescription,
                        IsBtnSandMailEnable = isBtnSandMailEnable
                    };

                    if (wind.Owner == null)
                    {
                        var viewService = IoC.Instance.Resolve<IViewService>();
                        try
                        {
                            var owner = viewService.GetActiveWindow();
                            if (owner != null)
                                wind.Owner = owner;
                        }
                        finally
                        {
                            if (wind.Owner == null)
                                wind.Topmost = true; //крайний случай
                        }
                    }

                    wind.Activate();
                    wind.ShowDialog();
                }));
        }

        internal static MailInfo CreateMail(string[] to, string subject, string header, string message, Exception ex, Dictionary<string, string> currentAdditionalParams, string[] attachments)
        {
            var result = new MailInfo
            {
                To = to,
                Subject = subject,
                Body = CreateMailBody(header, message, ex, currentAdditionalParams),
                AttachedFiles = CreateAttachedFileInfos(attachments)
            };
            return result;
        }

        private static string CreateMailBody(string header, string message, Exception ex, Dictionary<string, string> currentAdditionalParams)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(header))
                sb.AppendLine(header);

            sb.AppendLine(string.Format("{{cut {0}}}", BugReportResources.TechnicalInfo));
            //sb.AppendLine(BugReportResources.Separator01); //"---------------------------------------------------------"
            //sb.AppendLine(BugReportResources.DontEditThis); //"Внимание! Не редактируйте информацию, приведенную ниже."
            //sb.AppendLine(BugReportResources.Separator01);

            sb.AppendLine(string.Format("{0, -45}{1}", BugReportResources.ClientType, WMSEnvironment.Instance.ClientType));

            sb.AppendLine(string.Format("{0, -28}{1}", BugReportResources.Version, AssemblyAttributeAccessors.GetAssemblyFileVersion(Assembly.GetEntryAssembly())));

            sb.AppendLine(string.Format("{0, -25}{1}", BugReportResources.DotNetVersion, Environment.Version));

            sb.AppendLine(string.Format("{0, -24}{1}", BugReportResources.User, string.Concat(Environment.UserDomainName, "\\", Environment.UserName)));

            var machineName = Environment.MachineName;
            var client = WMSEnvironment.Instance.ClientCode;
            if (string.IsNullOrEmpty(client))
                client = machineName;
            if (client != machineName)
                sb.AppendLine(string.Format("{0, -24}{1}", BugReportResources.TerminalServer, machineName));

            sb.AppendLine(string.Format("{0, -35}{1}", BugReportResources.ComputerName, client));

            sb.AppendLine(string.Format("{0, -40}{1}", BugReportResources.OsVersion, Environment.OSVersion.VersionString));

            sb.AppendLine(string.Format("{0, -30}{1}", BugReportResources.Time, DateTime.Now));

            sb.AppendLine(string.Empty);

            if (StaticAdditionalParams.Count > 0)
            {
                foreach (var param in StaticAdditionalParams)
                {
                    sb.AppendLine(string.Format("{0}: {1}", param.Key, param.Value));
                }
            }

            if ((currentAdditionalParams != null) && (currentAdditionalParams.Count > 0))
            {
                sb.AppendLine(string.Empty);
                foreach (var param in currentAdditionalParams)
                {
                    sb.AppendLine(string.Format("{0}: {1}", param.Key, param.Value));
                }
            }
            sb.AppendLine(string.Empty);
            sb.AppendLine(BugReportResources.ExceptionMessage).AppendLine(message ?? string.Empty);
            sb.AppendLine(string.Format("{0}:{1}{2}", BugReportResources.Exception, Environment.NewLine, ex));
            sb.AppendLine("{cut}");

            return sb.ToString();
        }

        private static AttachedFileInfo[] CreateAttachedFileInfos(string[] attachments)
        {
            if (attachments == null || attachments.Length == 0)
                return null;

            var result = new List<AttachedFileInfo>();
            foreach (var fi in attachments.Where(f => !string.IsNullOrEmpty(f)).Select(f => new FileInfo(f)).Where(f => f.Exists))
            {
                byte[] buff;
                using (var fs = fi.OpenRead())
                {
                    using (var br = new BinaryReader(fs))
                    {
                        buff = br.ReadBytes((int)fi.Length);
                    }
                }
                result.Add(new AttachedFileInfo { Name = fi.Name, FileStream = buff });
            }
            return result.ToArray();
        }

        public static bool AddAdditionalParam(string key, string value)
        {
            if (StaticAdditionalParams.ContainsKey(key))
                return false;

            StaticAdditionalParams.Add(key, value);
            return true;
        }

        private static string GetStarMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;
            if (message.Length >= MaxLenght)
                return message.Left(MaxLenght - 3) + "...";
            return message;
        }

        #endregion .  Static methods  .
    }
}
