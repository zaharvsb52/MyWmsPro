using System.Windows;
using System.Windows.Controls;

namespace wmsMLC.DCL.Main.Views
{
    public partial class GridControlTemplate : UserControl
    {

        public GridControlTemplate()
        {
            InitializeComponent();
        }

        private void GridControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            //RestoreSetting();
        }

        //public void SaveSetting()
        //{
        //    SettingsHelper.SaveView(gridControlTemplate, GetNameControl());
        //}

        //public void RestoreSetting()
        //{
        //    SettingsHelper.RestoreView(gridControlTemplate, GetNameControl());
        //    //TODO нужно сделать добавление новых столбцов, если они появились в объекте и не сериализованы из Restore
        //}

        //public void CleanSetting()
        //{
        //    SettingsHelper.CleanView(GetNameControl());
        //}

        private string GetNameControl()
        {
            if (this.DataContext != null)
               return string.Format("{0}.{1}.{2}", gridControlTemplate.GetType(), gridControlTemplate.Name, this.DataContext.GetType().Name);
            return string.Format("{0}.{1}", gridControlTemplate.GetType(), gridControlTemplate.Name);
        }
    }

}
