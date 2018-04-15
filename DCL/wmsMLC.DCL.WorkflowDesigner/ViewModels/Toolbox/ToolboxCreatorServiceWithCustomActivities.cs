using System;
using System.Activities.Presentation.Toolbox;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Toolbox
{
    public class ToolboxCreatorServiceWithCustomActivities : ToolboxCreatorService
    {
        private IActivityLoader _loader;

        public ToolboxCreatorServiceWithCustomActivities(IActivityLoader loader)
        {
            _loader = loader;
        }

        public override IEnumerable<ToolboxCategory> GetToolboxCategories()
        {
            _loader.LoadActivities(false);

            var result = base.GetToolboxCategories().ToList();
            var cat = new ToolboxCategory("Custom Activities");

            foreach (var item in _loader.GetActivities())
            {
                //var name = item.Key;

                var designTimeVisibleAttribute = TypeDescriptor.GetAttributes(item.Value)[typeof(DesignTimeVisibleAttribute)] as DesignTimeVisibleAttribute;
                if (designTimeVisibleAttribute != null && !designTimeVisibleAttribute.Visible)
                    continue;

                object inst;
                if (item.Value.IsGenericType)
                {
                    var t1 = item.Value.GetGenericTypeDefinition();
                    inst = Activator.CreateInstance(t1.MakeGenericType(typeof(int)));
                }
                else
                    inst = Activator.CreateInstance(item.Value);

                var displayNameProperty = item.Value.GetProperty("DisplayName");
                var name = displayNameProperty.GetValue(inst, null).ToString();
                cat.Tools.Add(new ToolboxItemWrapper(item.Value, item.Key,  name));
            }

            result.Add(cat);
            return result;
        }
    }
}
