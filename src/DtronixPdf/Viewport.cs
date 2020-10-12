using System.Drawing;

namespace DtronixPdf
{
    public readonly struct Viewport
    {
        public ViewportOrigin OriginLocation { get; }
        public PointF Origin { get; }
        public SizeF Size { get; }
        public bool IsEmpty => Size.IsEmpty;

        public static Viewport Empty { get; } = new Viewport(0, 0, 0, 0, ViewportOrigin.Unset);

        public Viewport(PointF origin, SizeF size, ViewportOrigin originLocation)
        {
            OriginLocation = originLocation;
            Origin = origin;
            Size = size;
        }

        public Viewport(int x, int y, int width, int height, ViewportOrigin originLocation)
        {
            OriginLocation = originLocation;
            Origin = new Point(x, y);
            Size = new Size(width, height);
        }

    }
}