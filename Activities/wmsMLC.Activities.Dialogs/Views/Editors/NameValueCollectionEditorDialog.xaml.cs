using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Windows;

namespace wmsMLC.Activities.Dialogs.Views.Editors
{
    /// <summary>
    /// Interaction logic for NameValueCollectionEditorDialog.xaml
    /// </summary>
    public partial class NameValueCollectionEditorDialog
    {        
   

        public NameValueCollectionEditorDialog()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (ModelItem == null)
                return;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var options = new DynamicArgumentDesignerOptions
            {
                Title = string.Format("Укажите параметры")
            };

            var modelItem = ModelItem.Properties["Parameters"].Dictionary;

            using (ModelEditingScope change = modelItem.BeginEdit("ObjectEditing"))
            {
                if (DynamicArgumentDialog.ShowDialog(ModelItem, modelItem, Context, ModelItem.View, options))
                {
                    //TODO: Удалить свойства, у которых value == null.
                    change.Complete();
                }
                else
                {
                    change.Revert();
                }
            }
        }
    }
}
