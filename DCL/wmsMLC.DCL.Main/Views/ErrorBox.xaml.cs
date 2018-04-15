using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.ObjectBuilder2;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.Properties;
using wmsMLC.General.PL.WPF.Commands;
using log4net;
using wmsMLC.General.PL.WPF.Enums;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using GeneralResources = wmsMLC.General.Resources;

namespace wmsMLC.DCL.Main.Views
{
    public partial class ErrorBox : BaseDialogWindow, IHelpHandler, INotifyPropertyChanged
    {
        #region .  Static & Constants  .
        private const string ErrorMessageSubjectTemplate = "{0} application error";
        private static readonly ILog Log = LogManager.GetLogger(typeof(ErrorBox));
        #endregion .  Static & Constants  .

        #region .  Fields  .
        private static readonly Dictionary<string, string> StaticAdditionalParams = new Dictionary<string, string>();
        private bool _canSendMail;
        private List<MandantSdItem> _mandants;
        private MandantSdItem _selectedItem;
        #endregion .  Fields  .

        private ErrorBox()
        {
            ErrorSound = SystemSoundEnum.Hand;
            InitializeComponent();

            HelpServiceMail = Properties.Settings.Default.HelpServiceMail;
            SendMailCommand = new DelegateCustomCommand(this, OnSendMail, OnCanSendMail);
            CloseCommand = new DelegateCustomCommand(Close);
            LayoutRoot.DataContext = this;

            Loaded += OnLoaded;
        }

        #region .  Properties  .
        public string HelpServiceMail { get; set; }

        public bool CanSendMail 
        {
            get { return _canSendMail; }
            set
            {
                if (_canSendMail == value)
                    return;
                _canSendMail = value;
                OnPropertyChanged("CanSendMail");
            }
        }

        private Visibility _mandantListVisibility;
        public Visibility MandantListVisibility
        {
            get { return _mandantListVisibility; }
            set
            {
                if (_mandantListVisibility == value)
                    return;
                _mandantListVisibility = value;
                OnPropertyChanged("MandantListVisibility");
            }
        }

        public string Message { get; set; }

        public Exception Exception { get; set; }

        public string[] Attachments { get; set; }

        public Dictionary<string, string> CurrentAdditionalParams { get; set; }

        public List<MandantSdItem> Mandants
        {
            get
            {
                if (_mandants == null)
                    RefreshMandants();
                return _mandants;
            }
        }

        public MandantSdItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public ICommand SendMailCommand { get; private set; }

        public ICommand CloseCommand { get; private set; }

        public SystemSoundEnum ErrorSound { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            if (ErrorSound != SystemSoundEnum.None)
                new SoundHelper().PlaySystemSound(ErrorSound);
        }

        private void SendMail()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(BugReportResources.MessageToUser);
                sb.AppendLine("1. ");
                sb.AppendLine("2. ");
                sb.AppendLine("... ");
                sb.AppendLine(Environment.NewLine);
                sb.AppendLine(string.Format("{{cut {0}}}", BugReportResources.TechnicalInfo));
                sb.AppendLine(BugReportResources.Separator01); //"---------------------------------------------------------"
                sb.AppendLine(BugReportResources.DontEditThis); //"Внимание! Не редактируйте информацию, приведенную ниже."
                sb.AppendLine(BugReportResources.Separator01);

                sb.AppendLine(string.Format("{0, -27}{1}", BugReportResources.Version, AssemblyAttributeAccessors.GetAssemblyFileVersion(Assembly.GetEntryAssembly())));
                
                sb.AppendLine(string.Format("{0, -24}{1}", BugReportResources.DotNetVersion, Environment.Version));
                
                sb.AppendLine(string.Format("{0, -24}{1}", BugReportResources.User, string.Concat(Environment.UserDomainName, "\\", Environment.UserName)));

                var machineName = Environment.MachineName;
                var client = WMSEnvironment.Instance.ClientCode;
                if (string.IsNullOrEmpty(client))
                    client = machineName;
                if (client != machineName)
                    sb.AppendLine(string.Format("{0, -24}{1}", BugReportResources.TerminalServer, machineName));
                sb.AppendLine(string.Format("{0, -35}{1}", BugReportResources.ComputerName, Environment.MachineName));

                sb.AppendLine(string.Format("{0, -40}{1}", BugReportResources.OsVersion, Environment.OSVersion.VersionString));

                sb.AppendLine(string.Format("{0, -30}{1}", BugReportResources.Time, DateTime.Now));

                sb.AppendLine(string.Empty);
                if (SelectedItem != null)
                    sb.AppendLine(string.Format("{0}: {1}", BugReportResources.MandantCode, SelectedItem.MandantCode));

                if (StaticAdditionalParams.Count > 0)
                {
                    foreach (var param in StaticAdditionalParams)
                    {
                        sb.AppendLine(string.Format("{0}: {1}", param.Key, param.Value));
                    }
                }

                if ((CurrentAdditionalParams != null) && (CurrentAdditionalParams.Count > 0))
                {
                    sb.AppendLine(string.Empty);
                    foreach (var param in CurrentAdditionalParams)
                    {
                        sb.AppendLine(string.Format("{0}: {1}", param.Key, param.Value));
                    }
                }
                sb.AppendLine(string.Empty);
                sb.AppendLine(BugReportResources.ExceptionMessage).AppendLine(Message ?? string.Empty);
                sb.AppendLine(string.Format("{0}:{1}{2}", BugReportResources.Exception, Environment.NewLine, Exception));
                sb.AppendLine("{cut}");

                Mapi.SendMail(
                    HelpServiceMail,
                    string.Empty,
                    string.Format(ErrorMessageSubjectTemplate, AssemblyAttributeAccessors.AssemblyProduct),
                    sb.ToString(),
                    Attachments,
                    true);
            }
            catch (Exception ex)
            {
                Close();
                //ShowError("Невозможно отправить письмо в службу технической поддержки", ex, false);
                ShowError(message: BugReportResources.UnableSendEmailToSupport, ex: ex, additionalParams: null,
                    attachments: null, isBtnSandMailEnable: false,
                    mandantCode: SelectedItem == null ? null : SelectedItem.MandantCode);
            }
        }

        private bool OnCanSendMail()
        {
            return CanSendMail && SelectedItem != null && !string.IsNullOrEmpty(SelectedItem.MandantCode);
        }

        private void OnSendMail()
        {
            if (!OnCanSendMail())
                return;
            SendMail();
        }

        public void RefreshMandants()
        {
            if (_mandants == null)
                _mandants = new List<MandantSdItem>();
            if (_mandants.Any())
                _mandants.Clear();

            try
            {
                Mandant[] mandants;
                var attrEntity = FilterHelper.GetAttrEntity<Mandant>(Mandant.MANDANTCODEPropertyName, Mandant.MANDANTNAMEPropertyName);
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Mandant>>())
                {
                    mandants = mgr.GetFiltered(null, attrEntity).ToArray();
                }

                foreach (var mandant in mandants)
                {
                    _mandants.Add(new MandantSdItem { MandantCode = mandant.MandantCode, MandantName = mandant.MandantName });
                }
                //_mandants = _mandants.OrderBy(p => p.MandantName).ToList();
            }
            catch (Exception ex)
            {
                Log.WarnFormat("In method 'RefreshMandants' the error is occurred: {0}", ExceptionHelper.ExceptionToString(ex));
                Log.Debug(ex);
            }
            finally
            {
                if (!_mandants.Any())
                {
                    const string all = "All";
                    _mandants.Add(new MandantSdItem { MandantCode = all, MandantName = all });
                }
            }
        }
        #endregion . Methods .

        #region .  Static methods  .

        internal static void ShowError(string message, Exception ex, bool isBtnSandMailEnable = true)
        {
            ShowError(message, ex, null, null, isBtnSandMailEnable);
        }

        internal static void ShowError(string message, Exception ex, string[] attachments, Dictionary<string, string> additionalParams, bool isBtnSandMailEnable = true, string mandantCode = null, bool isMandantListVisible = true)
        {
            // первая ошибка содержит описание
            var trueEx = ExceptionHelper.GetFirstMeaningException(ex);
            var exMessage = ExceptionHelper.GetErrorMessage(trueEx);
            if (!string.IsNullOrEmpty(exMessage))
                Log.Debug(exMessage);

            // пробуем вытащить пояснение пользователю: оно может быть во вложенной ошибке
            var exReason = trueEx.InnerException is ICustomException
            ? string.Format(GeneralResources.ExceptionResources.ErrorReason, ExceptionHelper.GetErrorMessage(trueEx.InnerException))
            : string.Empty;

            // собираем подробную информацию
            var exDetails = ExceptionHelper.ExceptionToString(ex);
            if (exDetails == exMessage)
                exDetails = null;

            //var totalDescription = string.Format("{0}{1}{2}", 
            //    string.IsNullOrEmpty(message) ? string.Empty : string.Format("{0}{1}", message, Environment.NewLine),
            //    string.IsNullOrEmpty(exReason) ? string.Empty : string.Format("{0}{1}", exReason, Environment.NewLine),
            //    exDetails);

            var totalDescription = string.Format("{0}{1}{2}",
                string.IsNullOrEmpty(message) ? string.Empty : string.Format("{0} ", message),
                string.IsNullOrEmpty(exReason) ? string.Empty : string.Format("{0}{1}{0} ", Environment.NewLine, exReason),
                exDetails);

            // Проверка, что нет открытого окна с такой же ошибкой. Сообщение добавлем в уже открытое
            if (!string.IsNullOrEmpty(exMessage))
            {
                var collection = Application.Current.MainWindow.OwnedWindows;
                var res = collection
                    .OfType<ErrorBox>()
                    .FirstOrDefault(w => exMessage.Equals(w.LabelErrorCaption.Text) && w.MemoEditMainTxt.Text != null && !w.MemoEditMainTxt.Text.Contains(exReason));

                if (res != null)
                {
                    if (!string.IsNullOrEmpty(exReason))
                        res.MemoEditMainTxt.Text += totalDescription;
                    return;
                }
            }

            var wind = new ErrorBox
            {
                Message = message,
                Exception = ex,
                Attachments = attachments,
                CurrentAdditionalParams = additionalParams,
                Owner = Application.Current.MainWindow.IsLoaded ? Application.Current.MainWindow : null,
                CanSendMail = isBtnSandMailEnable,
                LabelErrorCaption = {Text = exMessage},
                MemoEditMainTxt = {Text = totalDescription},
                MandantListVisibility = isMandantListVisible ? Visibility.Visible : Visibility.Collapsed,
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
                        wind.Topmost = true;  //крайний случай
                }
            }

            wind.RefreshMandants();
            if (!string.IsNullOrEmpty(mandantCode) && wind.Mandants != null)
                wind.SelectedItem = wind.Mandants.FirstOrDefault(p => mandantCode.EqIgnoreCase(p.MandantCode));

            wind.ShowDialog();
        }

        public static string GetReason(Exception ex)
        {
            if (ex == null || ex.InnerException == null)
                return StringResources.ErrorUnexpected;

            string result;
            if (ex.InnerException is DeveloperException)
                result = StringResources.ErrorUnexpected;
            else
                result = ExceptionHelper.GetErrorMessage(ex.InnerException);

            return string.Format(GeneralResources.ExceptionResources.ErrorReason, result);
        }

        public static bool AddAdditionalParam(string key, string value)
        {
            if (StaticAdditionalParams.ContainsKey(key))
                return false;

            StaticAdditionalParams.Add(key, value);
            return true;
        }

        #region Logger
        public static string GetLogMessage(Exception ex, bool isStackIncluded = true, bool isstackendmessageincluded = true)
        {
            var unknown = StringResources.Unknown;
            if (ex == null) return unknown;
            Func<Exception, string> getLogMessageHandler =
                exc => exc == null ? null : string.Format("{0}: {1}", exc.GetType().FullName, exc.Message);
            var result = getLogMessageHandler(ex);
            var exce = ex;
            while (exce.InnerException != null)
            {
                result += string.Format(" ---> {0}", getLogMessageHandler(exce.InnerException));
                exce = exce.InnerException;
            }
            if (isStackIncluded)
                result += string.Format("{0}{1}", Environment.NewLine, GetStackTrace(ex, isstackendmessageincluded));
            return result;
        }

        public static string GetStackTrace(Exception ex, bool isstackendmessageincluded = true)
        {
            if (ex == null) return string.Empty;
            var exc = ex;
            var stack = new List<Exception> { exc };
            while (exc.InnerException != null)
            {
                stack.Add(exc.InnerException);
                exc = exc.InnerException;
            }

            var result = string.Empty;
            stack.Where(p => p != null)
                    .Select(p => new { p.StackTrace })
                    .Where(p => !string.IsNullOrEmpty(p.StackTrace))
                    .ForEach(p =>
                    {
                        // ReSharper disable AccessToModifiedClosure
                        result += string.Format("{0}{1}", p.StackTrace, Environment.NewLine);
                        // ReSharper restore AccessToModifiedClosure
                    });
            if (isstackendmessageincluded)
            {
                result += StringResources.ErrorStackTrace;
            }
            else
            {
                var length = result.Length - Environment.NewLine.Length;
                if (length > 0)
                {
                    result = result.Left(length);
                }
                else
                {
                    return string.Empty;
                }
            }
            return result;
        }

        public static Type GetDeclaringType(MethodBase source)
        {
            return source == null ? null : source.DeclaringType;
        }

        public static string GetMethodName(MethodBase source)
        {
            return source == null ? null : source.Name;
        }

        //public static string GetSource(MethodBase source)
        //{
        //    var type = source.GetDeclaringType();
        //    return string.Format("{0}.{1}", type == null ? null : type.FullName, source.GetMethodName());
        //}

        #endregion Logger
        #endregion .  Static methods  .

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion .  INotifyPropertyChanged  .

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return "wmsMLC";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "Error";
        }
        #endregion

        public class MandantSdItem
        {
            public string MandantCode { get; set; }
            public string MandantName { get; set; }
            public string DisplayText { get { return string.Format("{0} ({1})", MandantName, MandantCode); } }
        }
    }
}
