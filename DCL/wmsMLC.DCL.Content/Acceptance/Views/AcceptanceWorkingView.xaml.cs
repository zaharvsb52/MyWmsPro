using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Acceptance.ViewModels;

namespace wmsMLC.DCL.Content.Acceptance.Views
{
    /// <summary>
    /// Interaction logic for AcceptanceWorkingView.xaml
    /// </summary>
    public partial class AcceptanceWorkingView
    {
        public AcceptanceWorkingView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;

            ViewWorking.SetItemType(typeof(Working));
        }

        private AcceptanceWorkingViewModel ViewModel { get { return (AcceptanceWorkingViewModel) DataContext; } }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ViewWorking.Fields = ViewModel.Fields;
            ViewWorking.ParentViewModelSource = ViewModel.ParentViewModelSource;
            var vm = DataContext as wmsMLC.DCL.General.ViewModels.ICustomListViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
                ViewWorking.ParentViewModel = vm;
            }
        }
    }
}
