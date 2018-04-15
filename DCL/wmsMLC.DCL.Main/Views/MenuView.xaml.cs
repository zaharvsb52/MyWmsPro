using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;

namespace wmsMLC.DCL.Main.Views
{
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            IntialBars = new ObservableCollection<BarItem>();
            InitializeComponent();
        }

        public object Menu
        {
            get { return GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }
        public static readonly DependencyProperty MenuProperty = DependencyProperty.Register("Menu", typeof(object), typeof(MenuView), new PropertyMetadata(OnMenuPropertyChanged));

        private static void OnMenuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuView) d).OnMenuPropertyChanged(e.NewValue as MenuViewModel);
        }

        private void OnMenuPropertyChanged(MenuViewModel newvalue)
        {
            if (newvalue == null)
                return;

            foreach (var bar in IntialBars)
            {
                newvalue.Bars.Add(bar);
            }
        }

        public ObservableCollection<BarItem> IntialBars { get; private set; }
    }
}
