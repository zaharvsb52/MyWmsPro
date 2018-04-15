using System.Activities.Presentation.Metadata;

namespace wmsMLC.Activities.Business.Designer
{
    public sealed class ActivityLibraryMetadata : IRegisterMetadata
    {
        public void Register()
        {
            RegisterAll();
        }

        public static void RegisterAll()
        {
            var builder = new AttributeTableBuilder();
            ExecuteProcedureActivityDesigner.RegisterMetadata(builder);
            RegEventDesigner.RegisterMetadata(builder);
            CommentActivityDesigner.RegisterMetadata(builder);
            MultipleDynamicAssignActivityDesigner.RegisterMetadata(builder);
            DynamicAssignActivityDesigner.RegisterMetadata(builder);
            //ExecuteBusinessProcessActivityDesigner.RegisterMetadata(builder);
            CreateExpandoObjectViewModelActivityDesigner.RegisterMetadata(builder);
            // TODO: Other activities can be added here
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}