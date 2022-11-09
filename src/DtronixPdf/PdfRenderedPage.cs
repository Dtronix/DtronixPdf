using System;
using System.Threading.Tasks;

namespace DtronixPdf
{
    public class PdfRenderedPage : IDisposable
    {
        public PdfBitmap PdfBitmap { get; }
        //public BitmapSource BitmapSource { get; }

        public PdfRenderedPage(PdfBitmap pdfBitmap)
        {
            PdfBitmap = pdfBitmap ?? throw new ArgumentNullException(nameof(pdfBitmap));
            /*
            BitmapSource = BitmapSource.Create(
                pdfBitmap.Width,
                pdfBitmap.Height,
                72,
                72,
                PixelFormats.Bgra32,
                null,
                pdfBitmap.Scan0,
                pdfBitmap.Stride * pdfBitmap.Height,
                pdfBitmap.Stride);*/
        }

        public void Dispose()
        {
            PdfBitmap?.Dispose();
        }
    }
}
