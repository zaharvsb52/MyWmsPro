using System.Windows;
using System.Windows.Input;
using wmsMLC.DCL.Packing.ViewModels;

namespace wmsMLC.DCL.Packing.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PutInView
    {
       public PutInView()
        {
           InitializeComponent();

           if (Owner == null && Application.Current.MainWindow.IsActive)
           {
               Owner = Application.Current.MainWindow;
           }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void PutInView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Enter && e.Key != Key.Return) || (DataContext == null))
                return;

            var vm = DataContext as IPitInViewModel;
            if (vm == null)
                return;

            e.Handled = true;

            if (!vm.ByFill)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                return;
            }
            
            DialogResult = true;
            Close();
        }
    }
}
