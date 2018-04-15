using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Topology.ViewModels
{
    public class AreaModel : BaseTopologyBoxObject, ISourceModel
    {
        private readonly IList<Segment> _segments;
        private readonly IList<Place> _places;
        private const double AreaHeight = 0.2;
        private const double AreaPadding = 0.2;

        public Area Area { get; private set; }

        object ISourceModel.Source { get { return Area; } }

        protected override Color FillColor
        {
            get { return (Color)ColorConverter.ConvertFromString("#ffd861"); }
        }

        protected override string GetLabelText()
        {
            return Area == null ? "NULL" : Area.AreaCode;
        }

        public AreaModel(Area area, IList<Segment> segments, IList<Place> places)
        {
            Area = area;
            _segments = segments;
            _places = places;
            
            Build();
        }

        protected override void CreateModel()
        {
            foreach (var segment in _segments)
            {
                var segmentPlaces = _places.Where(p => p.SegmentCode == segment.SegmentCode).ToList();
                var segmentModel = new SegmentModel(segment, segmentPlaces);
                var transform = new Transform3DGroup();
                transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), Convert.ToDouble(segment.SegmentAngle))));
                transform.Children.Add(new TranslateTransform3D(ScaleValue(segment.SegmentPosX * 10) + AreaPadding, ScaleValue(segment.SegmentPosY * 10) + AreaPadding, AreaHeight));
                segmentModel.Transform = transform;
                Children.Add(segmentModel);
            }

            var bounds = Children.FindBounds();
            Length = bounds.SizeX + AreaPadding * 2;
            Width = bounds.SizeY + AreaPadding * 2;
            Height = AreaHeight;
            Center = new Point3D(bounds.X + Length / 2 - AreaPadding, bounds.Y + Width / 2 - AreaPadding, Height / 2);
        }

        public override string ToString()
        {
            return Area != null ? Area.AreaCode : base.ToString();
        }
    }
}