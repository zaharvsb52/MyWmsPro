using System;
using System.Windows.Media;
using DevExpress.Data.Linq;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Topology.ViewModels.Visitors
{
    public class PlaceStatesSelectorVisitor : IVisitor
    {
        private readonly bool _isNeedSet;

        public static Brush BusyFill = new SolidColorBrush(Colors.LightCoral);
        public static Brush ReserveFill = new SolidColorBrush(Colors.Yellow);
        public static double DefaultOpacity = 0.25;

        public PlaceStatesSelectorVisitor(bool isNeedSet)
        {
            _isNeedSet = isNeedSet;
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

            var state = (PlaceStates)Enum.Parse(typeof(PlaceStates), placeModel.Place.StatusCode_R);
            switch (state)
            {
                case PlaceStates.PLC_BUSY:
                    placeModel.Fill = BusyFill;
                    break;
                case PlaceStates.PLC_RESERV:
                    placeModel.Fill = ReserveFill;
                    break;
                default:
                    if (placeModel.Fill.IsFrozen)
                    {
                        var myFill = placeModel.Fill.Clone();
                        myFill.Opacity = DefaultOpacity;
                        placeModel.Fill = myFill;
                    }
                    else
                    {
                        placeModel.Fill.Opacity = DefaultOpacity;
                    }
                    break;
            }
        }
    }
}