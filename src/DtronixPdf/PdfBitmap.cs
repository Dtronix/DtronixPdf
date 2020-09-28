using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using DtronixPdf.Renderer.Dispatcher;
using PDFiumCore;

namespace DtronixPdf
{
    public class PdfBitmap : IAsyncDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;
        private readonly ThreadDispatcher _dispatcher;

        public PixelFormat Type => PixelFormat.Format32bppPArgb;

        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public IntPtr Scan0 { get; }

        public Bitmap Bitmap { get; }

        public bool IsDisposed { get; private set; }

        internal PdfBitmap(FpdfBitmapT pdfBitmap, int width, int height, ThreadDispatcher dispatcher)
        {
            _pdfBitmap = pdfBitmap;
            _dispatcher = dispatcher;
            Scan0 = fpdfview.FPDFBitmapGetBuffer(pdfBitmap);
            Stride = fpdfview.FPDFBitmapGetStride(pdfBitmap);
            Height = height;
            Width = width;
            Bitmap = new Bitmap(width, height, Stride, PixelFormat.Format32bppPArgb, Scan0);
        }

        public async ValueTask DisposeAsync()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Bitmap?.Dispose();

            await _dispatcher.QueueForCompletion(() =>
            {
                fpdfview.FPDFBitmapDestroy(_pdfBitmap);
            });

        }
    }
}