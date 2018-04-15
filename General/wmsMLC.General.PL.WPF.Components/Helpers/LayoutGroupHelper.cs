using System.Windows.Controls;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.General.PL.Model;

namespace wmsMLC.General.PL.WPF.Components.Helpers
{
    public static class LayoutGroupHelper
    {
        public static LayoutGroup CreateLayoutGroup(string name, LayoutGroupView view, Orientation orientation, LayoutItemLabelsAlignment itemLabelsAlignment)
        {
            return new LayoutGroup
            {
                Name = name,
                View = view,
                Orientation = orientation,
                ItemLabelsAlignment = itemLabelsAlignment
            };
        }

        public static LayoutGroup CreateLayoutGroup(string name)
        {
            return CreateLayoutGroup(name, LayoutGroupView.Group, Orientation.Vertical, LayoutItemLabelsAlignment.Local);
        }

        public static string GetLayoutGroupNameFromField(ValueDataField valueDataField, bool isWfDesignMode)
        {
            string svalue;
            return DataField.TryGetFieldProperties(valueDataField, ValueDataFieldConstants.LayoutGroupName,
                isWfDesignMode, out svalue)
                ? svalue
                : null;
        }
    }
}
