using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using DtronixPdf.Dispatcher;
using DtronixPdf.Renderer.Dispatcher;
using PDFiumCore;

namespace DtronixPdf
{
    public class PdfBitmap : IAsyncDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;

        private readonly ThreadDispatcher _dispatcher;

        public int Width { get; }

        public int Height { get; }

        public PixelFormat Format { get; }

        public int Stride { get; }

        public IntPtr Scan0 { get; }

        public float Scale { get; }

        public Viewport Viewport { get; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Only call within the dispatcher since dll calls are made.
        /// </summary>
        /// <param name="pdfBitmap"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="dispatcher"></param>
        /// <param name="format"></param>
        /// <param name="scale"></param>
        /// <param name="viewport"></param>
        internal PdfBitmap(
            FpdfBitmapT pdfBitmap, 
            int width, 
            int height, 
            ThreadDispatcher dispatcher, 
            PixelFormat format, 
            float scale, 
            Viewport viewport)
        {
            _pdfBitmap = pdfBitmap;
            _dispatcher = dispatcher;
            Scan0 = fpdfview.FPDFBitmapGetBuffer(pdfBitmap);
            Stride = fpdfview.FPDFBitmapGetStride(pdfBitmap);
            Height = height;
            Format = format;
            Scale = scale;
            Viewport = viewport;
            Width = width;
        }

        public Bitmap ToBitmap()
        {
            return new Bitmap(Width, Height, Stride, Format, Scan0);
        }

        public async ValueTask DisposeAsync()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            await _dispatcher.QueueForCompletion(() =>
            {
                fpdfview.FPDFBitmapDestroy(_pdfBitmap);
            });
        }
    }
}