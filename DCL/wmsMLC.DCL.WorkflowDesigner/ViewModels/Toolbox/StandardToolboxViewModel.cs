using System;
using System.Activities.Presentation.Toolbox;
using System.Linq;
using System.Windows.Threading;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Toolbox
{
    public class StandardToolboxViewModel : IToolboxViewModel
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        public ToolboxControl ToolBox { get; private set; }
        private IToolboxCreatorService _service;

        public StandardToolboxViewModel(IToolboxCreatorService service)
        {
            ToolBox = new ToolboxControl();
            if (service != null)
            {
                _service = service;
                ReloadToolboxIcons(_service);
            }
        }

        public void ReloadToolboxIcons(IToolboxCreatorService toolboxService)
        {
            var categories = toolboxService.GetToolboxCategories();

            _dispatcher.BeginInvoke(new Action(() =>
            {
                ToolBox.Categories.Clear();
                categories.ToList().ForEach(cat => ToolBox.Categories.Add(cat));
            }));
        }

        public void ReloadToolboxIcons()
        {
            ReloadToolboxIcons(_service);
        }

        public void UnloadToolboxIcons()
        {
            _dispatcher.BeginInvoke(new Action(() => ToolBox.Categories.Clear()));
        }
    }
}
