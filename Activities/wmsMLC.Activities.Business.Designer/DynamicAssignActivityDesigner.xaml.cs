using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.Activities.General;
using wmsMLC.General;

namespace wmsMLC.Activities.Business.Designer
{
    public partial class DynamicAssignActivityDesigner
    {
        /// <summary>
        /// Регистрация атрибутов компонента
        /// </summary>
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(DynamicAssign<>), new DesignerAttribute(typeof(DynamicAssignActivityDesigner)));
            //builder.AddCustomAttributes(typeof(DynamicAssign<>), new ToolboxBitmapAttribute(typeof(DynamicAssign<>), "DCLDefault16.png"));
            //builder.AddCustomAttributes(typeof(DynamicAssign<>), new CategoryAttribute(@"General"));
            builder.AddCustomAttributes(typeof(DynamicAssign<>), new DisplayNameAttribute(@"Присвоить значение полю"));
            builder.AddCustomAttributes(typeof(DynamicAssign<>), new DescriptionAttribute(@"Присваивает значение указанному полю сущности"));
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof (PropertyDescriptor[]), typeof (DynamicAssignActivityDesigner), new PropertyMetadata(default(PropertyDescriptor[])));

        public PropertyDescriptor[] Properties
        {
            get { return (PropertyDescriptor[]) GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        public DynamicAssignActivityDesigner()
        {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            if (this.ModelItem == null)
                return;

            // получаем св-ва объекта
            var genericArgs = this.ModelItem.ItemType.GetGenericArguments();
            if (genericArgs.Length != 1)
                throw new DeveloperException("Ожидался generic type с одним парметром");
            var objType = genericArgs[0];
            Properties = TypeDescriptor.GetProperties(objType).Cast<PropertyDescriptor>().ToArray();

            // обновляем выбранное значение из Combobox
            var ex = System.Windows.Data.BindingOperations.GetBindingExpression(cbProperties, System.Windows.Controls.ComboBox.SelectedValueProperty);
            if (ex != null)
            {
                ex.UpdateTarget();
            }
        }
    }
}
