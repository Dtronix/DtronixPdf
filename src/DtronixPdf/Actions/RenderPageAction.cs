using System;
using System.Net.Mime;
using System.Threading;
using DtronixCommon;
using DtronixCommon.Threading.Dispatcher;
using DtronixCommon.Threading.Dispatcher.Actions;
using PDFiumCore;

namespace DtronixPdf.Actions
{
    internal class RenderPageAction : MessagePumpActionResult<PdfBitmap>
    {
        public readonly FpdfPageT _pageInstance;
        private readonly float _scale;
        private readonly Boundary _viewport;
        private readonly RenderFlags _flags;
        private readonly uint? _backgroundColor = UInt32.MaxValue;
        private readonly ThreadDispatcher _dispatcher;
        private FpdfBitmapT _bitmap;

        public RenderPageAction(ThreadDispatcher dispatcher,
            FpdfPageT pageInstance,
            float scale,
            Boundary viewport,
            RenderFlags flags,
            uint? backgroundColor,
            CancellationToken cancellationToken) 
            : base(cancellationToken)
        {
            _pageInstance = pageInstance;
            _scale = scale;
            _viewport = viewport;
            _flags = flags;
            _backgroundColor = backgroundColor ?? UInt32.MaxValue;
            _dispatcher = dispatcher;
        }

        protected override unsafe PdfBitmap OnExecute(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _bitmap = fpdfview.FPDFBitmapCreateEx(
                    (int)_viewport.Width,
                    (int)_viewport.Height,
                    (int)FPDFBitmapFormat.BGRA,
                    IntPtr.Zero,
                    0);

                if (_bitmap == null)
                    throw new Exception("failed to create a bitmap object");

                cancellationToken.ThrowIfCancellationRequested();
                if (_backgroundColor.HasValue)
                {
                    fpdfview.FPDFBitmapFillRect(
                        _bitmap,
                        0, 
                        0, 
                        (int)_viewport.Width,
                        (int)_viewport.Height,
                        _backgroundColor.Value);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                using var clipping = new FS_RECTF_
                {
                    Left = 0,
                    Right = _viewport.Width,
                    Bottom = 0,
                    Top = _viewport.Height
                };

                // |          | a b 0 |
                // | matrix = | c d 0 |
                // |          | e f 1 |
                using var matrix = new FS_MATRIX_
                {
                    A = _scale,
                    B = 0,
                    C = 0,
                    D = _scale,
                    E = -_viewport.MinX,
                    F = -_viewport.MinY
                };

                fpdfview.FPDF_RenderPageBitmapWithMatrix(_bitmap, _pageInstance, matrix, clipping, (int)_flags);

                cancellationToken.ThrowIfCancellationRequested();

                return new PdfBitmap(_bitmap, _dispatcher, _scale, _viewport);
            }
            catch (OperationCanceledException)
            {
                if(_bitmap != null)
                    fpdfview.FPDFBitmapDestroy(_bitmap);
                throw;
            }
            catch (Exception ex)
            {
                if (_bitmap != null)
                    fpdfview.FPDFBitmapDestroy(_bitmap);

                throw new Exception("Error rendering page. Check inner exception.", ex);
            }
        }
    }
}