using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using wmsMLC.Activities.Dialogs.Views.Editors;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Activities
{
    public class CreateMenuActivity : NativeActivity<ValueDataField[]>
    {
        private Collection<ValueDataField> _menuItems;

        public CreateMenuActivity()
        {
            DisplayName = "Создание меню";
        }

        [DisplayName(@"Меню")]
        [Editor(typeof(MenuCollectionPropertyEditor), typeof(DialogPropertyValueEditor))]
        public Collection<ValueDataField> MenuItems
        {
            get { return _menuItems ?? (_menuItems = new Collection<ValueDataField>()); }
            set { _menuItems = value; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var result = new List<ValueDataField>();
            foreach (var field in MenuItems)
            {
                field.FieldName = field.Name;
                field.SourceName = field.Name;
            }
            result.AddRange(MenuItems);
            context.SetValue(Result, result.ToArray());
        }
    }
}
