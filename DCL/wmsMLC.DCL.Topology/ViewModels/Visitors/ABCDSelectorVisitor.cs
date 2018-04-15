using System.Windows.Media;

namespace wmsMLC.DCL.Topology.ViewModels.Visitors
{
    public class ABCDSelectorVisitor : IVisitor
    {
        private readonly bool _isNeedSet;
        public static Brush BrushForA = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aed8ff"));
        public static Brush BrushForB = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#62aaec"));
        public static Brush BrushForC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#186ab5"));
        public static Brush BrushForD = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#07233c"));

        public ABCDSelectorVisitor(bool isNeedShowFree)
        {
            _isNeedSet = isNeedShowFree;
        }

        public void Visit(object obj)
        {
            var placeModel = obj as PlaceModel;
            if (placeModel == null)
                return;

            if (!_isNeedSet)
            {
                placeModel.SetDefaultFill();
                return;
            }

            if (placeModel.Place.PlaceSortA > 0)
                placeModel.Fill = BrushForA;
            else if (placeModel.Place.PlaceSortB > 0)
                placeModel.Fill = BrushForB;
            else if (placeModel.Place.PlaceSortC > 0)
                placeModel.Fill = BrushForC;
            else if (placeModel.Place.PlaceSortD > 0)
                placeModel.Fill = BrushForD;
        }
    }
}