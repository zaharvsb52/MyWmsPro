using wmsMLC.General;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class AboutBoxViewModel : ViewModelBase
    {
        public string Product { get { return AssemblyAttributeAccessors.AssemblyProduct; } }

        public string Version
        {
            get
            {
                var dbversion = WMSEnvironment.Instance.DbSystemInfo == null
                    ? null
                    : WMSEnvironment.Instance.DbSystemInfo.Version;
                return string.Format("{0}{1}", AssemblyAttributeAccessors.AssemblyFileVersion,
                    string.IsNullOrEmpty(dbversion) ? null : string.Format(" (DB: {0})", dbversion));
            }
        }
        public string Copyright { get { return AssemblyAttributeAccessors.AssemblyCopyright; } }
        public string AllModules { get { return AssemblyAttributeAccessors.AssemblyAll; } }
        public string Email { get { return Properties.Settings.Default.HelpServiceMail; } }
    }
}