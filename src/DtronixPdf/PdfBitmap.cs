using System;
using System.Threading.Tasks;
using DtronixCommon;
using DtronixCommon.Threading.Dispatcher;
using DtronixCommon.Threading.Dispatcher.Actions;
using PDFiumCore;

namespace DtronixPdf
{
    public class PdfBitmap : IDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;

        private readonly PdfActionSynchronizer _synchronizer;

        public float Scale { get; }

        public Boundary Viewport { get; }

        public IntPtr Pointer { get; }

        public int Stride { get; }

        public int Width { get; }

        public int Height { get; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Only call within the synchronizer since dll calls are made.
        /// </summary>
        /// <param name="pdfBitmap"></param>
        /// <param name="image"></param>
        /// <param name="synchronizer"></param>
        /// <param name="scale"></param>
        /// <param name="viewport"></param>
        internal PdfBitmap(
            FpdfBitmapT pdfBitmap,
            PdfActionSynchronizer synchronizer,
            float scale, 
            Boundary viewport)
        {
            _pdfBitmap = pdfBitmap;
            Stride = fpdfview.FPDFBitmapGetStride(_pdfBitmap);
            Width = (int)viewport.Width;
            Height = (int)viewport.Height;
            Pointer = fpdfview.FPDFBitmapGetBuffer(_pdfBitmap);
            _synchronizer = synchronizer;
            Scale = scale;
            Viewport = viewport;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            _synchronizer.SyncExec(() => fpdfview.FPDFBitmapDestroy(_pdfBitmap));
        }
    }
}
