using System;

namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IDesignerViewModel
    {
        void ReloadDesigner(object root);
        void ReloadDesigner(string file);
        object CurrentSurface { get; }

        event Action SurfaceChanged;
    }
}
