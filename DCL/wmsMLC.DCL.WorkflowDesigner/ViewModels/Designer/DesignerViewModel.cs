using System;
using System.Activities.Core.Presentation;
using System.Activities.Presentation.Model;
using System.ComponentModel;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Designer
{
    public class DesignerViewModel : INotifyPropertyChanged, IDesignerViewModel, ILoadErrorDesignerViewModel
    {
        public DesignerViewModel()
        {
            this.CurrentSurface = new EmptyDesignerSurface();
        }

        public object CurrentSurface
        {
            get;
            private set;
        }

        public event Action SurfaceChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void ReloadError(string xaml)
        {
            this.CurrentSurface = new LoadErrorDesignerSurface(xaml);
            DispatcherHelper.Invoke(new Action(this.OnSurfaceChanged));
        }

        public void ReloadDesigner(object root)
        {
            System.Activities.Presentation.WorkflowDesigner designer = null;
            DispatcherHelper.Invoke(new Action(() => designer = ReloadDesignerSTA(root, false)));
            this.CurrentSurface = new DesignerSurface(designer);
            DispatcherHelper.Invoke(new Action(this.OnSurfaceChanged));
        }

        public void ReloadDesigner(string file)
        {
            System.Activities.Presentation.WorkflowDesigner designer = null;

            DispatcherHelper.Invoke(new Action(() => designer = ReloadDesignerSTA(file, true)));
            this.CurrentSurface = new DesignerSurface(designer);
            DispatcherHelper.Invoke(new Action(this.OnSurfaceChanged));
        }

        private System.Activities.Presentation.WorkflowDesigner ReloadDesignerSTA(object root, bool fromFile)
        {
            var result = new System.Activities.Presentation.WorkflowDesigner();
            new DesignerMetadata().Register();            
            if (!fromFile)
            {
                result.Load(root);
            }
            else
            {
                result.Load(root.ToString());
            }
            SetAssembly(result);
            return result;
        }

        private void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void OnSurfaceChanged()
        {
            this.OnPropertyChanged("CurrentSurface");

            if (this.SurfaceChanged != null)
            {
                this.SurfaceChanged();
            }
        }

        private void SetAssembly(System.Activities.Presentation.WorkflowDesigner wd)
        {
            var mtm = wd.Context.Services.GetService<ModelTreeManager>();
            var ab = mtm.Root;
        }
    }
}
