using System.Windows;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;

namespace wmsMLC.DCL.Configurator.Views.Controls
{
    public class ConfiguratorComboBoxEdit : ComboBoxEdit
    {
        public CriteriaOperator LookupFilterCriteria
        {
            get { return (CriteriaOperator)GetValue(LookupFilterCriteriaProperty); }
            set { SetValue(LookupFilterCriteriaProperty, value); }
        }

        public static readonly DependencyProperty LookupFilterCriteriaProperty = DependencyProperty.Register("LookupFilterCriteria", typeof(CriteriaOperator), typeof(ConfiguratorComboBoxEdit));

        protected override void OnPopupOpened()
        {
            base.OnPopupOpened();
            if (EditMode == EditMode.InplaceActive && ListBox != null)
            {
                //К сожалению необходимо создавать ip каждый раз, в противном случае имеют место проблемы с SelectedItems
                var ip = new ItemsProvider(Settings) {DisplayFilterCriteria = LookupFilterCriteria};
                ListBox.ItemsSource = ip.VisibleListSource;

                //if ((SelectedItems == null || SelectedItems.Count == 0) && EditValue != null)
                //    ListBox.SelectedItem = ip.GetItem(EditValue);
            }
        }
    }
}
