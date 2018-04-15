using System;
using System.Collections.Generic;
using System.Windows.Media;
using DevExpress.Xpf.Grid;

namespace wmsMLC.DCL.Main.Helpers
{
    public class ImageSelector : TreeListNodeImageSelector
    {
        private static Dictionary<Type, Func<object, ImageSource>> _getImageStrategies = new Dictionary<Type, Func<object, ImageSource>>();

        public static void AddGetImageStrategy(Type type, Func<object, ImageSource> getImageStrategy)
        {
            _getImageStrategies.Add(type, getImageStrategy);
        }

        public override ImageSource Select(DevExpress.Xpf.Grid.TreeList.TreeListRowData rowData)
        {
            if (rowData == null || rowData.Row == null)
                return base.Select(rowData);

            var type = rowData.Row.GetType();
            if (!_getImageStrategies.ContainsKey(type))
                return base.Select(rowData);

            return _getImageStrategies[type](rowData.Row);
        }
    }
}