using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Topology.ViewModels;

namespace wmsMLC.DCL.Topology.Views
{
    public partial class TopologyView : DXPanelView, IHelpHandler
    {
        public TopologyView()
        {
            InitializeComponent();
            hv.LookAt(new Point3D(-16.348, -10.863, 16.599), new Vector3D(0.865, 0.47, -0.175), 1.0 );
        }

        private bool _moving;
        private void HViewPortMouseDown(object sender, MouseButtonEventArgs e)
        {
            // first see if they have selected an item only allow selection of drillholes
            var helix = (HelixViewport3D)sender;
            var position = e.GetPosition(helix);
            var obj = helix.FindNearestVisual(position);
            if (obj != null)
            {
                _moving = true;
                ((TopologyViewModel)this.DataContext).SelectedObject = obj;

            }
        }

        private void HViewPortMouseMove(object sender, MouseEventArgs e)
        {
            if (!_moving)
                return;

            var helix = (HelixViewport3D)sender;
            var position = e.GetPosition(helix);
            var obj = helix.FindNearestPoint(position);
        }

        private void HViewPortMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_moving)
                _moving = false;
        }

        private void HViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((TopologyViewModel) this.DataContext).ShowCardForSelectedObject();
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return null;
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "Topology";
        }
        #endregion
    }
}
