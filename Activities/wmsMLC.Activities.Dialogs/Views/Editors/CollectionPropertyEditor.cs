using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using wmsMLC.General;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Views.Editors
{
    public class CollectionPropertyEditor : DialogPropertyValueEditor
    {
        public CollectionPropertyEditor()
        {
            //http://stackoverflow.com/questions/4517170/create-a-grid-in-wpf-as-template-programmatically
            InlineEditorTemplate = new DataTemplate();

            var grid = new FrameworkElementFactory(typeof(Grid));
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
            grid.AppendChild(col1);
            grid.AppendChild(col2);

            var label = new FrameworkElementFactory(typeof(Label));
            var labelBinding = new Binding("Value");
            label.SetValue(ContentControl.ContentProperty, labelBinding);
            label.SetValue(Grid.ColumnProperty, 0);
            grid.AppendChild(label);

            var editModeSwitchButton = new FrameworkElementFactory(typeof(EditModeSwitchButton));
            editModeSwitchButton.SetValue(Grid.ColumnProperty, 1);
            editModeSwitchButton.SetValue(EditModeSwitchButton.TargetEditModeProperty, PropertyContainerEditMode.Dialog);
            grid.AppendChild(editModeSwitchButton);

            InlineEditorTemplate.VisualTree = grid;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            var modelItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false /*no parent*/, null) as ModelItem;
            if (modelItem == null)
                throw new DeveloperException("Ошибки получения context в диалоге.");

            var modelFields = modelItem.Properties["Fields"];
            if (modelFields == null)
                throw new DeveloperException("Can't find property 'Fields'.");

            var collection = modelFields.Collection;
            if (collection == null)
                throw new DeveloperException("Property 'Fields' is not Collection.");

            var actualCollection = collection.GetCurrentValue() as IEnumerable<ValueDataField>;
            if (actualCollection == null)
                return;

            var clonedCollection = new ObservableCollection<ValueDataField>(actualCollection.Select(p => (ValueDataField)p.Clone()));

            var dialog = new CollectionEditorDialog(clonedCollection);
            if (dialog.ShowDialog() == true)
            {
                using (var scope = collection.BeginEdit())
                {
                    collection.Clear();
                    dialog.Collection.ToList().ForEach(p => collection.Add(p));
                    scope.Complete();
                }
            }
        }
    }
}
