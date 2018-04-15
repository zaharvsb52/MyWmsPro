using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Topology.ViewModels
{
    public class SegmentModel : BaseTopologyBoxObject, ISourceModel
    {
        private readonly IList<Place> _places;
        private const double SegmentHeight = 0.2;
        private const double SegmentPadding = 0.2;

        public Segment Segment { get; private set; }

        object ISourceModel.Source { get { return Segment; } }

        protected override Color FillColor
        {
            get { return (Color)ColorConverter.ConvertFromString("#ff8961"); }
        }

        protected override string GetLabelText()
        {
            return Segment == null ? "NULL" : Segment.SegmentCode;
        }

        public SegmentModel(Segment segment, IList<Place> places)
        {
            Segment = segment;
            _places = places;
            
            Build();
        }

        protected override void CreateModel()
        {
            foreach (var place in _places)
            {
                var placeModel = new PlaceModel(place);
                placeModel.Transform = new TranslateTransform3D(ScaleValue(place.PosX * 10) + SegmentPadding, SegmentPadding, ScaleValue(place.PosY * 10) + SegmentHeight);
                Children.Add(placeModel);
            }

            var bounds = Children.FindBounds();
            Length = bounds.SizeX + SegmentPadding * 2;
            Width = bounds.SizeY + SegmentPadding * 2;
            Height = SegmentHeight;
            Center = new Point3D(Length / 2, Width / 2, Height / 2);
        }

        public override string ToString()
        {
            return Segment != null ? Segment.SegmentCode : base.ToString();
        }
    }
}