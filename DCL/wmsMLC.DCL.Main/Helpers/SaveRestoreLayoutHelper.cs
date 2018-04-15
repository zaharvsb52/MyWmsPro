using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using log4net;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Helpers
{
    /// <summary>
    /// Помощь при сохранении настроек различных контролов.
    /// NOTE: Реализация стратегий сохранения задается извне через RegisterSaveRestoreStrategy. 
    /// </summary>
    //TODO: Вынести стратегию именования настроек (перейти от именования файлов к именованию настроек, чтобы легче было перейти на сохранение в базе)
    public static class SaveRestoreLayoutHelper
    {
        #region .  Fields  .
        private static ILog _log = LogManager.GetLogger(typeof(SaveRestoreLayoutHelper));
        private static readonly Dictionary<Type, Action<SaveRestoreContext>> SaveResotreStrategies = new Dictionary<Type, Action<SaveRestoreContext>>();
        public const string VersionPrefix = "$ver";

        #endregion .  Fields  .

        #region .  Methods  .
        public static void RegisterSaveRestoreStrategy(Type type, Action<SaveRestoreContext> action)
        {
            SaveResotreStrategies.Add(type, action);
        }

        public static bool SaveLayout(FrameworkElement element, FormComponents formComponents)
        {
            return SaveLayout(element, null, formComponents);
        }

        public static bool SaveLayout(FrameworkElement element, Dictionary<string, FileAppearanceSettingsDb> filesDb, FormComponents formComponents)
        {
            return RunActions(element, SaveRestoreActionType.Save, filesDb, formComponents);
        }

        public static bool RestoreLayout(FrameworkElement element, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            return RestoreLayout(element, null, formComponents, actionProcessingObjectTypes, doNotActionProcessingObjectTypes);
        }

        public static bool RestoreLayout(FrameworkElement element, Dictionary<string, FileAppearanceSettingsDb> filesDb, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            return RunActions(element, SaveRestoreActionType.Restore, filesDb, formComponents, actionProcessingObjectTypes, doNotActionProcessingObjectTypes);
        }

        public static bool ClearLayout(FrameworkElement element, FormComponents formComponents)
        {
            return ClearLayout(element, null, formComponents);
        }

        public static bool ClearLayout(FrameworkElement element, Dictionary<string, FileAppearanceSettingsDb> filesDb, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            return RunActions(element, SaveRestoreActionType.Clear, filesDb, formComponents, actionProcessingObjectTypes, doNotActionProcessingObjectTypes);
        }

        private static object GetDataContext(FrameworkElement element)
        {
            if (element == null)
                return null;

            var window = element as Window;
            if (window != null)
            {
                var content = window.Content as FrameworkElement;
                if (content != null)
                    return content.DataContext;
            }

            return element.DataContext;
        }

        public static Version GetVersionByFile(string fl, string mask)
        {
            if (string.IsNullOrEmpty(fl))
                return null;

            var strVersion = Path.GetFileNameWithoutExtension(fl);
            strVersion = strVersion.Replace(string.Format("{0}{1}", mask, VersionPrefix), "");

            Version version;
            if (!Version.TryParse(strVersion, out version))
                version = null;
            return version;
        }

        public static string GetFullFileNameClient(string path, string mask, Version version)
        {
            return Path.Combine(path,
                (version != null ? string.Format("{0}{1}{2}.xml", mask, VersionPrefix, version) : string.Format("{0}{1}0.0.0.0.xml", mask, VersionPrefix)));
        }

        //1-ый элемент массива основной
        private static string[] GetMaskFileName(FrameworkElement element)
        {
            const string thatisWindow = "Window_";

            var datacontext0 = GetDataContext(element);
            var datacontexts = new List<object> {datacontext0};
            var iswindow = element is Window;

            if (datacontext0 != null && !iswindow && element is CustomBarManager)
            {
                //1-ый элемент - модель, 2-ой - модель меню (глобальный)
                var menuHandler = datacontext0 as IMenuHandler;
                if (menuHandler != null && menuHandler.Menu != null && !menuHandler.Menu.NotUseGlobalLayoutSettings)
                    datacontexts.Add(menuHandler.Menu);
            }

            var result = new List<string>();

            foreach (var datacontext in datacontexts)
            {
                var dataName = (datacontext == null)
                    ? "NULL"
                    : string.Format("{0}{1}", iswindow ? thatisWindow : null,
                        datacontext.GetType().GetFullNameWithoutVersion());

                var dataSuffix = GetFirstSettingsNameHandler(element, datacontext);
                if (dataSuffix != null)
                {
                    var dataSuffixStr = dataSuffix.GetSuffix();
                    if (!string.IsNullOrEmpty(dataSuffixStr))
                        dataName += string.Format(".{0}{1}", iswindow ? thatisWindow : null, dataSuffixStr);
                }

                var controlName = string.IsNullOrEmpty(element.Name) ? element.GetType().GetFullNameWithoutVersion() : element.Name;
                var controlSuffix = element as ISettingsNameHandler;
                if (controlSuffix != null)
                {
                    var controlSuffixStr = controlSuffix.GetSuffix();
                    if (!string.IsNullOrEmpty(controlSuffixStr))
                        controlName += string.Format(".{0}{1}", iswindow ? thatisWindow : null, controlSuffixStr);
                }

                result.Add(GetMd5Code(string.Format("{0}.{1}", dataName, controlName)));
            }

            if (result.Count == 1 && !string.IsNullOrEmpty(result[0]))
            {
                if (element is CustomGridControl || element is CustomTreeListControl)
                {
                    result.Add(string.Format("{0}_ExpressionStyle", result[0]));
                }
                else if (iswindow)
                {
                    result.Add(string.Format("{0}_WindowPosition", result[0]));
                }
            }
            return result.ToArray();
        }

        private static ISettingsNameHandler GetFirstSettingsNameHandler(FrameworkElement element, object datacontext = null)
        {
            if (element == null)
                return null;

            var dt = datacontext ?? GetDataContext(element);
            var snh = dt as ISettingsNameHandler;
            if (snh != null)
                return snh;

            var parent = element.Parent as FrameworkElement;
            if (parent == null)
                return null;

            return GetFirstSettingsNameHandler(parent);
        }

        private static void AnalysisMask(string path, string mask, out string currentFile, out Version version)
        {
            var dir = new DirectoryInfo(path);
            var fileArray = dir.GetFiles(string.Format("{0}{1}*.xml", mask, VersionPrefix));
            currentFile = string.Empty;
            version = null;

            if (fileArray.Length == 1)
            {
                currentFile = fileArray[0].FullName;
                version = GetVersionByFile(fileArray[0].FullName, mask) ?? new Version(0, 0, 0, 0);
            }
            else if (fileArray.Length == 0)
            {
                fileArray = dir.GetFiles(string.Format("{0}.xml", mask));
                if (fileArray.Length > 0)
                {
                    currentFile = fileArray[0].FullName;
                    version = new Version(0, 0, 0, 0);
                }

            }
            else
            {
                //TODO: Что делать если их несколько вдруг
                foreach (var fileInfo in fileArray)
                {
                    try
                    {
                        fileInfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }
                }
            }
        }

        public static FileAppearanceSettingsClient[] GetSettingsFullFileName(FrameworkElement el)
        {
            //return GetDefaultRootPath() + string.Format("{0:x}.xml", (dataName + "." + controlName).GetHashCode());
            var result = new List<FileAppearanceSettingsClient>();

            var path = GetDefaultRootPath();
            var masks = GetMaskFileName(el);

            foreach (var mask in masks.Where(p => !string.IsNullOrEmpty(p)))
            {
                string currentFile;
                Version version;
                AnalysisMask(path, mask, out currentFile, out version);
                result.Add(new FileAppearanceSettingsClient(path, mask, version, currentFile));
            }

            return result.ToArray();
        }

        public static Version GetMaxDbVersion(Dictionary<string, FileAppearanceSettingsDb> filesDb)
        {
            if (filesDb == null)
                return null;

            Version maxVersion = null;
            foreach (var flDb in filesDb.Values)
            {
                if (maxVersion == null || flDb.Version > maxVersion)
                {
                    maxVersion = (Version)flDb.Version.Clone();
                }
            }

            return maxVersion;
        }

        public static string GetMd5Code(string text)
        {
            var bytetext = Encoding.UTF8.GetBytes(text);
            using (var md5 = MD5.Create())
            {
                var byteHashed = md5.ComputeHash(bytetext);
                return ToHex(byteHashed, false);
            }
        }

        private static string ToHex(byte[] bytes, bool upperCase)
        {
            var result = new StringBuilder(bytes.Length * 2);

            for (var i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }

        private static string _rootPathCache;

        public static string GetDefaultRootPath()
        {
            if (!string.IsNullOrEmpty(_rootPathCache))
                return _rootPathCache;

            var execAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var atts = execAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            if (atts.Length != 1)
                throw new DeveloperException(DeveloperExceptionResources.CantGetDefaultPathCompanyNotFound);

            var company = ((AssemblyCompanyAttribute)atts[0]).Company;
            string product;
            atts = execAssembly.GetCustomAttributes(typeof(AssemblyDefaultAliasAttribute), true);
            if (atts.Length != 1)
            {
                atts = execAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
                if (atts.Length != 1)
                    throw new DeveloperException(DeveloperExceptionResources.CantGetDefaultPathProductNotFound);

                product = ((AssemblyProductAttribute)atts[0]).Product;
            }
            else
                product = ((AssemblyDefaultAliasAttribute)atts[0]).DefaultAlias;

            const string appdata = "APPDATA";
            var appDataFolder = Environment.GetEnvironmentVariable(appdata);
            if (string.IsNullOrEmpty(appDataFolder))
                throw new DeveloperException("Environment variable '{0}' is not defined.", appdata);

            var result = Path.Combine(appDataFolder, company, product);
            var environment = WMSEnvironment.Instance.DbSystemInfo.Environment;
            if (!string.IsNullOrEmpty(environment))
            {
                foreach (var ch in Path.GetInvalidPathChars())
                {
                    environment = environment.Replace(ch.ToString(CultureInfo.InvariantCulture), string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(environment))
                result = Path.Combine(result, environment);

            _rootPathCache = result;
            return _rootPathCache;
        }

        private static bool RunActions(FrameworkElement element, SaveRestoreActionType actionType, Dictionary<string, FileAppearanceSettingsDb> filesDb, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            var actions = GetActionsForObj(element);
            if (actions.Length == 0)
                return false;

            var context = new SaveRestoreContext(element, GetSettingsFullFileName(element), true, actionType, filesDb, formComponents, actionProcessingObjectTypes, doNotActionProcessingObjectTypes);
            foreach (var action in actions)
                action(context);

            return true;
        }

        private static Action<SaveRestoreContext>[] GetActionsForObj(FrameworkElement obj)
        {
            return SaveResotreStrategies.Keys
                                        .Where(i => i.IsInstanceOfType(obj))
                                        .Select(i => SaveResotreStrategies[i])
                                        .ToArray();
        }
        #endregion .  Methods  .
    }

    /// <summary>
    /// Контекст сохранения/загрузки. Содержит все необходимое, чтобы стратегия могла спокойно отработать.
    /// </summary>
    /// 
    public class FileAppearanceSettingsClient
    {
        public string Root { get; private set; }
        public string Mask { get; set; }
        public Version Version { get; set; }
        public string CurrentFullFileName { get; set; }

        public FileAppearanceSettingsClient(string root, string mask, Version version, string currentFullFileName)
        {
            Mask = mask;
            Version = version;
            CurrentFullFileName = currentFullFileName;
            Root = root;
        }

        public string GetFullFileNameClient()
        {
            return SaveRestoreLayoutHelper.GetFullFileNameClient(Root, Mask, Version);
        }

        public string GetFileNameBd()
        {
            return string.Format("{0}.xml", Mask);
        }

        public void SetCurrentFileName()
        {
            CurrentFullFileName = GetFullFileNameClient();
        }
    }

    public class FileAppearanceSettingsDb
    {
        public Byte[] Text { get; set; }
        public Version Version { get; set; }

        public FileAppearanceSettingsDb(Byte[] text, Version version = null)
        {
            Text = text;
            Version = version;
        }
    }

    public class SaveRestoreContext
    {
        public SaveRestoreContext(FrameworkElement element, FileAppearanceSettingsClient[] fileNames, bool overwriteIfExits, SaveRestoreActionType actionType, Dictionary<string, FileAppearanceSettingsDb> filesDb, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            ProcessingObject = element;
            FileNames = fileNames;
            OverwriteIfExits = overwriteIfExits;
            ActionType = actionType;
            FilesDb = filesDb;
            FormComponents = formComponents;
            ActionProcessingObjectTypes = actionProcessingObjectTypes;
            DoNotActionProcessingObjectTypes = doNotActionProcessingObjectTypes;
        }

        #region .  Properties  .
        public FrameworkElement ProcessingObject { get; private set; }
        public FileAppearanceSettingsClient[] FileNames { get; private set; }
        public bool OverwriteIfExits { get; private set; }
        public SaveRestoreActionType ActionType { get; private set; }
        public Dictionary<string, FileAppearanceSettingsDb> FilesDb { get; private set; }
        public FormComponents FormComponents { get; private set; }
        public Type[] ActionProcessingObjectTypes { get; private set; }
        public Type[] DoNotActionProcessingObjectTypes { get; private set; }
        #endregion .  Properties  .

        public bool ValidateAction()
        {
            if (ProcessingObject == null)
                return false;

            var actionProcessing = ActionProcessingObjectTypes == null
               ? new List<Type>()
               : new List<Type>(ActionProcessingObjectTypes);
            if ((FormComponents & FormComponents.Menu) == FormComponents.Menu)
            {
                actionProcessing.Add(typeof(ICustomBarManager));
            }
            if ((FormComponents & FormComponents.Components) == FormComponents.Components &&
                !(ProcessingObject is ICustomBarManager) && !(ProcessingObject is Window))
            {
                actionProcessing.Add(ProcessingObject.GetType());
            }
            if ((FormComponents & FormComponents.FormSize) == FormComponents.FormSize ||
                (FormComponents & FormComponents.FormPosition) == FormComponents.FormPosition)
            {
                actionProcessing.Add(typeof (Window));
            }

            var result = ValidateActionProcessing(actionProcessing);
            if (!result)
                return false;

            var doNotActionProcessing = DoNotActionProcessingObjectTypes == null
                ? new List<Type>()
                : new List<Type>(DoNotActionProcessingObjectTypes);
            
            if ((FormComponents & FormComponents.Menu) != FormComponents.Menu)
            {
                doNotActionProcessing.Add(typeof(ICustomBarManager));
            }
            if ((FormComponents & FormComponents.Components) != FormComponents.Components &&
                !(ProcessingObject is ICustomBarManager) && !(ProcessingObject is Window))
            {
                doNotActionProcessing.Add(ProcessingObject.GetType());
            }
            if ((FormComponents & FormComponents.FormSize) != FormComponents.FormSize &&
                (FormComponents & FormComponents.FormPosition) != FormComponents.FormPosition)
            {
                doNotActionProcessing.Add(typeof(Window));
            }

            return !doNotActionProcessing.Any() || !ValidateActionProcessing(doNotActionProcessing);
        }

        private bool ValidateActionProcessing(ICollection<Type> actionProcessing)
        {
            if (actionProcessing == null || !actionProcessing.Any())
                return true;

            return actionProcessing.Where(p => p != null).Any(p => p.IsInstanceOfType(ProcessingObject));
        }
    }

    /// <summary>
    /// Тип действия
    /// </summary>
    public enum SaveRestoreActionType
    {
        Save,
        SaveDb,
        Restore,
        Clear
    }
}