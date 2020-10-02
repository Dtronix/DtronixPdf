using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DtronixPdf
{
    public struct Viewport
    {
         public readonly PointF Center;
         public readonly SizeF Size;

        public Viewport(PointF center, SizeF size)
        {
            Center = center;
            Size = size;
        }

        public Viewport(int x, int y, int width, int height)
        {
            Center = new Point(x, y);
            Size = new Size(width, height);
        }
    }
}
