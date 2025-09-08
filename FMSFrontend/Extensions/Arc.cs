using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FMSFrontend.Extensions
{
    public class Arc : Shape
    {
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(Arc),
                new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(nameof(EndAngle), typeof(double), typeof(Arc),
                new FrameworkPropertyMetadata(90d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsLargeArcProperty =
            DependencyProperty.Register(nameof(IsLargeArc), typeof(bool), typeof(Arc),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public double StartAngle { get => (double)GetValue(StartAngleProperty); set => SetValue(StartAngleProperty, value); }
        public double EndAngle { get => (double)GetValue(EndAngleProperty); set => SetValue(EndAngleProperty, value); }
        public bool IsLargeArc { get => (bool)GetValue(IsLargeArcProperty); set => SetValue(IsLargeArcProperty, value); }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var rect = new Rect(RenderSize);
                double cx = rect.Width / 2.0;
                double cy = rect.Height / 2.0;
                double r = Math.Min(cx, cy);

                Point Polar(double angleDeg)
                {
                    double rad = (Math.PI / 180.0) * angleDeg;
                    return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
                }

                var start = Polar(StartAngle);
                var end = Polar(EndAngle);
                bool isLarge = IsLargeArc;

                var figure = new PathFigure { StartPoint = start, IsFilled = false, IsClosed = false };
                var seg = new ArcSegment
                {
                    Point = end,
                    Size = new Size(r, r),
                    SweepDirection = SweepDirection.Clockwise,
                    IsLargeArc = isLarge
                };
                figure.Segments.Add(seg);

                var geo = new PathGeometry();
                geo.Figures.Add(figure);
                return geo;
            }
        }
    }
}
