using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using wmsMLC.Activities.Dialogs.Activities;

namespace wmsMLC.Activities.Business.Designer
{
    // Interaction logic for CommentActivityDesigner.xaml
    public partial class CommentActivityDesigner
    {
        /// <summary>
        /// Регистрация атрибутов компонента
        /// </summary>
        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(CommentActivity), new DesignerAttribute(typeof(CommentActivityDesigner)));
        }

        public CommentActivityDesigner()
        {
            InitializeComponent();
        }
    }
}
