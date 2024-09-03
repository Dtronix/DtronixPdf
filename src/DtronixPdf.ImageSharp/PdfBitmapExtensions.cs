using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace DtronixPdf.ImageSharp
{
    public static class PdfBitmapExtensions
    {
        public static unsafe Image<Bgra32> GetImage(this PdfBitmap bitmap)
        {
            return Image.WrapMemory<Bgra32>(
                bitmap.Pointer.ToPointer(),
                (int)(bitmap.Viewport[2] - bitmap.Viewport[0]),
                (int)(bitmap.Viewport[3] - bitmap.Viewport[1]));
        }

    }
}
