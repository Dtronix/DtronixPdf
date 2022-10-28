using System;
using System.Threading.Tasks;
using DtronixCommon;
using DtronixCommon.Threading.Dispatcher;
using DtronixCommon.Threading.Dispatcher.Actions;
using PDFiumCore;

namespace DtronixPdf
{
    public class PdfBitmap : IAsyncDisposable, IDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;

        private readonly ThreadDispatcher _dispatcher;

        public float Scale { get; }

        public Boundary Viewport { get; }

        public IntPtr Pointer { get; }

        public int Stride { get; }

        public int Width { get; }

        public int Height { get; }

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
            ThreadDispatcher dispatcher,
            float scale, 
            Boundary viewport)
        {
            _pdfBitmap = pdfBitmap;
            Stride = fpdfview.FPDFBitmapGetStride(_pdfBitmap);
            Width = (int)viewport.Width;
            Height = (int)viewport.Height;
            Pointer = fpdfview.FPDFBitmapGetBuffer(_pdfBitmap);
            _dispatcher = dispatcher;
            Scale = scale;
            Viewport = viewport;
        }



        public async ValueTask DisposeAsync()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            await _dispatcher.Queue(new SimpleMessagePumpAction(() =>
            {
                fpdfview.FPDFBitmapDestroy(_pdfBitmap);
            })).ConfigureAwait(false);
        }


        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            fpdfview.FPDFBitmapDestroy(_pdfBitmap);
        }
    }
}
