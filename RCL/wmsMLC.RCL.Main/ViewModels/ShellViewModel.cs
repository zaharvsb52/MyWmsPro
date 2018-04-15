using System.Windows;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.RCL.Main.Views;

namespace wmsMLC.RCL.Main.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private string _title;

        public ShellViewModel()
        {
            Title = string.Format("{0} - v.{1}", AssemblyAttributeAccessors.AssemblyProduct, AssemblyAttributeAccessors.AssemblyFileVersion);
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