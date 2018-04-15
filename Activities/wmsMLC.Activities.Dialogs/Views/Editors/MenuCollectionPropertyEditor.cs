using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Views.Editors
{
    public class MenuCollectionPropertyEditor : CollectionPropertyEditor
    {
        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            var modelItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false /*no parent*/, null) as ModelItem;
            var menuCollection = modelItem.Properties["MenuItems"].Collection;
            var actualCollection = menuCollection.GetCurrentValue() as IEnumerable<ValueDataField>;

            var clonedCollection = new ObservableCollection<ValueDataField>(actualCollection.Select((item) => (ValueDataField)item.Clone()));

            var dialog = new MenuCollectionEditorDialog(clonedCollection);
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                using (var scope = menuCollection.BeginEdit())
                {
                    menuCollection.Clear();
                    dialog.Collection.ToList().ForEach((item) => menuCollection.Add(item));
                    scope.Complete();
                }
            }
        }
    }
}
