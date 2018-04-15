namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Designer
{
    public class LoadErrorDesignerSurface : ILoadErrorDesignerSurface
    {
        public LoadErrorDesignerSurface(string xaml)
        {
            this.Xaml = xaml;
        }

        public string Xaml { get; set; }

        public string AlternativeText
        {
            get { return "Load error, workflow could not be displayed"; }
        }
    }
}
