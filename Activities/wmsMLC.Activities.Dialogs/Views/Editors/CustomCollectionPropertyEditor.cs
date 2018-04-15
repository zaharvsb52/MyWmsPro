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
    public class CustomCollectionPropertyEditor : CollectionPropertyEditor
    {        
        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            var modelItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false /*no parent*/, null) as ModelItem;
            var collection = modelItem.Properties["Fields"].Collection;
            var actualCollection = collection.GetCurrentValue() as IEnumerable<ValueDataField>;

            var clonedCollection = new ObservableCollection<ValueDataField>(actualCollection.Select((item) => item.Clone() as ValueDataField));

            var dialog = new CustomCollectionEditorDialog(clonedCollection);
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                using (var scope = collection.BeginEdit())
                {
                    collection.Clear();
                    dialog.Collection.ToList().ForEach((item) => collection.Add(item));
                    scope.Complete();
                }
            }
        }
    }
}
