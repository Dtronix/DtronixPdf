using System;
using System.Threading.Tasks;
using DtronixPdf.Dispatcher;
using DtronixPdf.Dispatcher.Actions;
using PDFiumCore;
using SixLabors.ImageSharp;

namespace DtronixPdf
{
    public class PdfBitmap : IAsyncDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;

        private readonly ThreadDispatcher _dispatcher;

        public float Scale { get; }

        public RectangleF Viewport { get; }

        public Image Image { get; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Only call within the dispatcher since dll calls are made.
        /// </summary>
        /// <param name="pdfBitmap"></param>
        /// <param name="image"></param>
        /// <param name="dispatcher"></param>
        /// <param name="scale"></param>
        /// <param name="viewport"></param>
        internal PdfBitmap(
            FpdfBitmapT pdfBitmap, 
            Image image,
            ThreadDispatcher dispatcher,
            float scale, 
            RectangleF viewport)
        {
            _pdfBitmap = pdfBitmap;
            _dispatcher = dispatcher;
            Scale = scale;
            Viewport = viewport;
            Image = image;
        }

        public async ValueTask DisposeAsync()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            await _dispatcher.QueueForCompletion(new SimpleMessagePumpAction(() =>
            {
                fpdfview.FPDFBitmapDestroy(_pdfBitmap);
            }));
        }
    }
}