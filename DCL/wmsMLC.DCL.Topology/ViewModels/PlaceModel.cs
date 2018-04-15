using System.Windows.Media;
using System.Windows.Media.Media3D;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Topology.ViewModels
{
    public class PlaceModel : BaseTopologyBoxObject, ISourceModel
    {
        public Place Place { get; private set; }

        object ISourceModel.Source { get { return Place; } }

        protected override Color FillColor
        {
            get { return (Color)ColorConverter.ConvertFromString("#61fffc"); }
        }

        protected override string GetLabelText()
        {
            return Place == null ? "NULL" : Place.PlaceName;
        }

        public PlaceModel(Place place)
        {
            Place = place;
            
            Build();
        }

        protected override void CreateModel()
        {
            Length = ScaleValue(Place.PlaceWidth);
            Width = ScaleValue(Place.PlaceLength);
            Height = ScaleValue(Place.PlaceHeight);
            Center = new Point3D(Length / 2, Width / 2, Height / 2);
        }

        public override string ToString()
        {
            if (Place != null)
            {
                return string.Format("{0} ({1}x{2:D3}x{3:D3})", Place, Place.PlaceCode.Length > 11 ? Place.PlaceCode.Substring(Place.PlaceCode.Length - 9, 3) : Place.PlaceCode, (int)Place.PlaceX, (int)Place.PlaceY);
            }

            return base.ToString();
        }
    }
}