using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Topology.ViewModels
{
    public class WarehouseModel : BaseTopologyBoxObject, ISourceModel
    {
        private readonly IList<Area> _areas;
        private readonly IList<Segment> _segments;
        private readonly IList<Place> _places;
        private const double WarehouseHeight = 0.2;
        private const double WarehousePadding = 0.2;

        public Warehouse Warehouse { get; private set; }

        object ISourceModel.Source { get { return Warehouse; } }

        protected override Color FillColor
        {
            get { return (Color)ColorConverter.ConvertFromString("#ad61ff"); }
        }

        protected override string GetLabelText()
        {
            return Warehouse == null ? "NULL" : Warehouse.WarehouseCode;
        }

        public WarehouseModel(Warehouse warehouse, IList<Area> areas, IList<Segment> segments, IList<Place> places)
        {
            Warehouse = warehouse;
            _areas = areas;
            _segments = segments;
            _places = places;

            Build();
        }

        protected override void CreateModel()
        {
            foreach (var area in _areas)
            {
                var areaSegments = _segments.Where(s => s.AreaCode_R == area.AreaCode).OrderBy(s => s.SegmentNumber).ToList();
                var areaPlaces = _places.Where(p => p.AreaCode == area.AreaCode).ToList();
                var areaModel = new AreaModel(area, areaSegments, areaPlaces);
                areaModel.Transform = new TranslateTransform3D(WarehousePadding, WarehousePadding, WarehouseHeight);
                Children.Add(areaModel);
            }

            var bounds = Children.FindBounds();
            Length = bounds.SizeX + WarehousePadding * 2;
            Width = bounds.SizeY + WarehousePadding * 2;
            Height = WarehouseHeight;
            Center = new Point3D(bounds.X + Length / 2 - WarehousePadding, bounds.Y + Width / 2 - WarehousePadding, Height / 2);
        }

        public override string ToString()
        {
            return Warehouse != null ? Warehouse.WarehouseCode : base.ToString();
        }
    }
}