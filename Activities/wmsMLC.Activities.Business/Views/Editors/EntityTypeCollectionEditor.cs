using System;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using wmsMLC.Activities.Dialogs.Views.Editors;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Business.Views.Editors
{
    public class EntityTypeCollectionEditor : CollectionPropertyEditor
    {
        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            var modelItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false /*no parent*/, null) as ModelItem;
            var menuCollection = modelItem.Properties["EntityTypes"].Collection;
            var actualCollection = menuCollection.GetCurrentValue() as IEnumerable<ValueDataField>;

            var clonedCollection = new ObservableCollection<ValueDataField>(actualCollection.Select((item) => (ValueDataField)item.Clone()));

            var dialog = new EntityTypeCollectionEditorDialog(clonedCollection);
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
