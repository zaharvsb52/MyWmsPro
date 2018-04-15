using System.Windows.Media;
using HelixToolkit.Wpf;

namespace wmsMLC.DCL.Topology.ViewModels
{
    public class FloorModel : BoxVisual3D
    {
        public FloorModel()
        {
            Fill = new SolidColorBrush(Colors.DarkGray);
        }
    }
}