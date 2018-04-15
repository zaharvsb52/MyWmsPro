using System;
using System.Security;
using Microsoft.Win32;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;

namespace wmsMLC.DCL.Browser.ViewModels
{
    public class BrowserViewModel : PanelViewModelBase
    {
        #region .  fields & consts  .

        private Uri _url;
        private static readonly string[] AppNameArr = {"wmsMLC.DCL.Client.exe", "wmsMLC.DCL.Client.vshost.exe"};

        private const string RegKeyName =
            "HKEY_CURRENT_USER\\Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION";

        private const string RegKeyNameForAllUsers =
            "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION";

        private const int RegKeyValue = 0x00002711;

        #endregion .  fields & consts  .

        #region . ctor  .

        public BrowserViewModel()
        {
            CheckOrSetRegKey();
        }

        #endregion . ctor  .

        #region .  properties  .

        public Uri Url
        {
            get { return _url; }
            set
            {
                _url = value;
                OnPropertyChanged("Url");
            }
        }

        #endregion .  properties  .

        #region .  methods  .

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            IsMenuEnable = true;
            IsCustomizeBarEnabled = true;

            AllowClosePanel = true;
        }

        private static void CheckOrSetRegKey()
        {
            const string errorMessage =
                "Невозможно настроить работу модуля Таможня, отсутсвует доступ на запись в реестр. Обратитесь в службу поддержки";

            try
            {
                foreach (var regKeyValueName in AppNameArr)
                {
                    if (Registry.GetValue(RegKeyName, regKeyValueName, null) == null)
                        Registry.SetValue(RegKeyName, regKeyValueName, RegKeyValue, RegistryValueKind.DWord);

                    if (Registry.GetValue(RegKeyNameForAllUsers, regKeyValueName, null) == null)
                        Registry.SetValue(RegKeyNameForAllUsers, regKeyValueName, RegKeyValue, RegistryValueKind.DWord);
                }
            }
            catch (SecurityException)
            {
                if (CheckRegRights())
                    throw new DeveloperException(errorMessage);
            }
            catch (UnauthorizedAccessException)
            {
                if (CheckRegRights())
                    throw new DeveloperException(errorMessage);
            }
        }

        private static bool CheckRegRights()
        {
            foreach (var regKeyValueName in AppNameArr)
            {
                if (Registry.GetValue(RegKeyNameForAllUsers, regKeyValueName, null) == null) continue;
                if (Registry.GetValue(RegKeyName, regKeyValueName, null) != null)
                    return true;
            }
            return false;
        }

        #endregion .  methods  .
    }
}