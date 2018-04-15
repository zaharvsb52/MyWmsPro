using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using DevExpress.Mvvm.Native;
using HelixToolkit.Wpf;
using wmsMLC.DCL.Topology.ViewModels.Visitors;

namespace wmsMLC.DCL.Topology.ViewModels
{
    public abstract class BaseTopologyBoxObject : BoxVisual3D, IVisitorAcceptor
    {
        protected const double FloorHeight = 0.2;
        protected double FloorPadding = 0.2;

        protected abstract Color FillColor
        {
            get;
        }

        protected virtual double FillOpacity
        {
            get { return 1; }
        }

        protected abstract string GetLabelText();

        protected virtual void Build()
        {
            SetDefaultFill();
            CreateModel();
            AddLabel();
        }

        public virtual void SetDefaultFill()
        {
            Fill = new SolidColorBrush(FillColor);
            if (Fill.IsFrozen)
            {
                var myFill = Fill.Clone();
                myFill.Opacity = FillOpacity;
                Fill = myFill;
            }
            else
            {
                Fill.Opacity = FillOpacity;
            }
        }

        protected virtual void CreateModel()
        {
        }

        public virtual void AddLabel()
        {
            if (Height < 0.2 || Length <= 0.8)
                return;

            var textLabel = new TextVisual3D();
            textLabel.FontWeight = FontWeights.Bold;
            textLabel.Position = new Point3D(Center.X, -0.1, Center.Z);
            textLabel.Height = 0.15;
            textLabel.TextDirection = new Vector3D(1, 0, 0);
            textLabel.Text = GetLabelText();
            Children.Add(textLabel);
        }

        protected static double ScaleValue(decimal valueInMillimeters)
        {
            return (double)valueInMillimeters / 500.0;
        }

        public virtual void AcceptVisitor(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}