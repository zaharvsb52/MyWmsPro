using System;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Toolbox
{
    public class ToolboxCreatorService : IToolboxCreatorService
    {
        private List<ToolboxCategory> _toolboxCategories;
        private List<ResourceReader> _resourceReaderList;

        private IEnumerable<ResourceReader> GetIconReaders()
        {
            if (this._resourceReaderList != null) return this._resourceReaderList;

            this._resourceReaderList = new List<ResourceReader>();
            
            var requiredFileList = new List<string>
            {
                "Microsoft.VisualStudio.Activities"
            };

            foreach (var item in requiredFileList)
            {
                var file = string.Format("{0}.dll", item);
                //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External", file);
                var resource = string.Format("{0}.Resources.resources", item);

                try
                {
                    var loadedAssm = AppDomain.CurrentDomain.GetAssemblies();
                    var assm = loadedAssm.FirstOrDefault(i => i.GetName().Name.Equals(item));
                    var assembly = assm ?? Assembly.Load(item);

                    var stream = assembly.GetManifestResourceStream(resource);

                    if (stream == null)
                    {
                        var message = string.Format("Resource '{0}' in File '{1}' not found!", resource, file);
                        throw new InvalidOperationException(message);
                    }

                    this._resourceReaderList.Add(new ResourceReader(stream));
                }
                catch (FileNotFoundException fnfEx)
                {
                    var message = string.Format("File '{0}' not found!", file);
                    throw new FileNotFoundException(message, file, fnfEx);
                }
            }

            return this._resourceReaderList;
        }

        public virtual IEnumerable<ToolboxCategory> GetToolboxCategories()
        {
            if (this._toolboxCategories != null) return this._toolboxCategories;

            this._toolboxCategories = new List<ToolboxCategory>
            {
                new ToolboxCategory("ControlFlow")
                {
                    new ToolboxItemWrapper(typeof(DoWhile), "DoWhile", "DoWhile"),                    
                    new ToolboxItemWrapper(typeof(System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory<>), "ForEach", "ForEach"),
                    //new ToolboxItemWrapper(typeof(ForEach<>), "ForEach", "ForEach"),
                    new ToolboxItemWrapper(typeof(If), "If", "If"),
                    new ToolboxItemWrapper(typeof(Parallel), "Parallel", "Parallel"),
                    //new ToolboxItemWrapper(typeof(ParallelForEach<>), "ParallelForEach", "ParallelForEach<T>"),
                    new ToolboxItemWrapper(typeof(System.Activities.Core.Presentation.Factories.ParallelForEachWithBodyFactory<>), "ParallelForEach", "ParallelForEach<T>"),
                    new ToolboxItemWrapper(typeof(Pick), "Pick", "Pick"),
                    new ToolboxItemWrapper(typeof(PickBranch), "PickBranch", "PickBranch"),         
                    new ToolboxItemWrapper(typeof(Sequence), "Sequence", "Sequence"),
                    new ToolboxItemWrapper(typeof(Switch<>), "Switch", "Switch<T>"),
                    new ToolboxItemWrapper(typeof(While), "While", "While")
                },
                new ToolboxCategory("Flowchart")
                {
                    new ToolboxItemWrapper(typeof(Flowchart), "Flowchart", "Flowchart"),
                    new ToolboxItemWrapper(typeof(FlowSwitch<>), "FlowSwitch", "FlowSwitch<T>"),
                    new ToolboxItemWrapper(typeof(FlowDecision), "FlowDecision", "FlowDecision")
                },
                new ToolboxCategory("Runtime")
                {
                    new ToolboxItemWrapper(typeof(Persist), "Persist", "Persist"),
                    new ToolboxItemWrapper(typeof(TerminateWorkflow), "TerminateWorkflow", "TerminateWorkflow")
                },
                new ToolboxCategory("Primitives")
                {
                    new ToolboxItemWrapper(typeof(Assign), "Assign", "Assign"),
                    new ToolboxItemWrapper(typeof(Assign<>), "Assign", "Assign<T>"),
                    new ToolboxItemWrapper(typeof(Delay), "Delay", "Delay"),
                    new ToolboxItemWrapper(typeof(InvokeMethod), "InvokeMethod", "InvokeMethod"),
                    new ToolboxItemWrapper(typeof(WriteLine), "WriteLine", "WriteLine")
                },
                new ToolboxCategory("Transaction")
                {
                    new ToolboxItemWrapper(typeof(CancellationScope), "CancellationScope", "CancellationScope"),
                    new ToolboxItemWrapper(typeof(CompensableActivity), "CompensableActivity", "CompensableActivity"),
                    new ToolboxItemWrapper(typeof(Compensate), "Compensate", "Compensate"),
                    new ToolboxItemWrapper(typeof(Confirm), "Confirm", "Confirm"),
                    new ToolboxItemWrapper(typeof(TransactionScope), "TransactionScope", "TransactionScope")
                },
                new ToolboxCategory("Collection")
                {
                    new ToolboxItemWrapper(typeof(AddToCollection<>), "AddToCollection", "AddToCollection<T>"),
                    new ToolboxItemWrapper(typeof(ClearCollection<>), "ClearCollection", "ClearCollection<T>"),
                    new ToolboxItemWrapper(typeof(ExistsInCollection<>), "ExistsInCollection", "ExistsInCollection<T>"),
                    new ToolboxItemWrapper(typeof(RemoveFromCollection<>), "RemoveFromCollection", "RemoveFromCollection<T>")
                },
                new ToolboxCategory("ErrorHandling")
                {
                    new ToolboxItemWrapper(typeof(Rethrow), "Rethrow", "Rethrow"),
                    new ToolboxItemWrapper(typeof(Throw), "Throw", "Throw"),
                    new ToolboxItemWrapper(typeof(TryCatch), "TryCatch", "TryCatch")
                }
            };

            var activityTypes = new List<ToolboxItemWrapper>();
            this._toolboxCategories.ForEach(cat => cat.Tools.ToList().ForEach(activityTypes.Add));
            ToolboxIconCreator.LoadToolboxIcons(activityTypes, this.GetIconReaders().ToList());

            return this._toolboxCategories;
        }
    }
}
