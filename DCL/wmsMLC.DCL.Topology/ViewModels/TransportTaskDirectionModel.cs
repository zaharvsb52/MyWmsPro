using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using HelixToolkit.Wpf;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Topology.ViewModels
{
    public class TransportTaskDirectionModel : ArrowVisual3D, ISourceModel, ISelectable
    {
        private readonly Func<string, List<PlaceModel>> _findPlaceAccessor;

        private readonly PlaceModel _startPlaceModel;
        private readonly PlaceModel _finishPlaceModel;

        public Brush _previosFill;
        public static Brush BrushForStartPlace = new SolidColorBrush(Colors.LightGreen);
        public static Brush BrushForFinishPlace = new SolidColorBrush(Colors.LightCoral);

        public static Brush DefaultBrushForArrow = new SolidColorBrush(Colors.Black);
        public static Brush CreatedBrushForArrow = new SolidColorBrush(Colors.Yellow);
        public static Brush ActivatedBrushForArrow = new SolidColorBrush(Colors.YellowGreen);
        public static Brush CompletedBrushForArrow = new SolidColorBrush(Colors.DarkGreen);

        public TransportTaskDirectionModel(TransportTask transportTask, Func<string, List<PlaceModel>> findPlaceAccessor)
        {
            _findPlaceAccessor = findPlaceAccessor;
            Source = transportTask;

            _startPlaceModel = _findPlaceAccessor(Source.TaskStartPlaceCode).FirstOrDefault();
            _finishPlaceModel = _findPlaceAccessor(Source.TaskFinishPlace).FirstOrDefault();

            if (_startPlaceModel == null || _finishPlaceModel == null)
            {
                IsValid = false;
                return;
            }
            
            IsValid = true;
            this.Point1 = _startPlaceModel.Center;
            this.Point2 = _finishPlaceModel.Center;
            this.Diameter = 0.05;

            switch (Source.StatusCode)
            {
                case TTaskStates.TTASK_CREATED:
                    Fill = CreatedBrushForArrow;
                    break;
                case TTaskStates.TTASK_ACTIVATED:
                    Fill = ActivatedBrushForArrow;
                    break;
                case TTaskStates.TTASK_COMPLETED:
                    Fill = CompletedBrushForArrow;
                    break;
                default:
                    Fill = DefaultBrushForArrow;
                    break;
            }
            if (Fill.IsFrozen)
            {
                var myFill = Fill.Clone();
                myFill.Opacity = 0.7;
                Fill = myFill;
            }
            else
            {
                Fill.Opacity = 0.7;
            }
        }

        public void ToDefaultFill()
        {
            _startPlaceModel.SetDefaultFill();
            _finishPlaceModel.SetDefaultFill();
        }

        public TransportTask Source { get; private set; }

        public bool IsValid { get; private set; }

        object ISourceModel.Source { get { return Source; } }
        
        void ISelectable.SetSelected()
        {
            _startPlaceModel.Fill = BrushForStartPlace;
            _finishPlaceModel.Fill = BrushForFinishPlace;
            _previosFill = Fill;
            if (Fill.IsFrozen)
            {
                var myFill = Fill.Clone();
                myFill.Opacity = 1;
                Fill = myFill;
            }
            else
            {
                Fill.Opacity = 1;
            }
        }

        void ISelectable.ClearSelected()
        {
            ToDefaultFill();
            Fill = _previosFill;
        }
    }
}