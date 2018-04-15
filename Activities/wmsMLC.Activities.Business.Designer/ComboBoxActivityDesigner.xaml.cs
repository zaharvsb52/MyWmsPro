using System.Windows.Controls.Primitives;

namespace wmsMLC.Activities.Business.Designer
{
    /// <summary>
    /// Interaction logic for ComboBoxActivityDesigner.xaml
    /// </summary>
    public partial class ComboBoxActivityDesigner
    {
        public ComboBoxActivityDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (ModelItem == null)
                return;

            // обновляем выбранное значение из Combobox
            var ex = System.Windows.Data.BindingOperations.GetBindingExpression(cbProperties, Selector.SelectedValueProperty);
            if (ex != null)
            {
                ex.UpdateTarget();
            }
        }


    }
}
