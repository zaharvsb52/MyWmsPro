using System.Windows;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private string _title;

        public ShellViewModel()
        {
            Title = string.Format("{0} - v.{1} ({2})",
                AssemblyAttributeAccessors.AssemblyProduct,
                AssemblyAttributeAccessors.AssemblyFileVersion,
                AssemblyAttributeAccessors.AssemblyVersion.Revision);
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private DependencyObject _mainView;
        public DependencyObject MainView
        {
            get { return _mainView ?? (_mainView = IoC.Instance.Resolve<MainView>()); }
        }
    }
}