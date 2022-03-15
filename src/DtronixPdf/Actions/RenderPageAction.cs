using System;
using System.Threading;
using DtronixCommon.Threading.Dispatcher;
using DtronixCommon.Threading.Dispatcher.Actions;
using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DtronixPdf.Actions
{
    internal class RenderPageAction : MessagePumpActionResult<PdfBitmap>
    {
        public readonly FpdfPageT _pageInstance;
        private readonly float _scale;
        private readonly RectangleF _viewport;
        private readonly RenderFlags _flags;
        private readonly Color? _backgroundColor;
        private readonly ThreadDispatcher _dispatcher;
        private FpdfBitmapT _bitmap;

        public RenderPageAction(ThreadDispatcher dispatcher,
            FpdfPageT pageInstance,
            float scale,
            RectangleF viewport,
            RenderFlags flags,
            Color? backgroundColor,
            CancellationToken cancellationToken) 
            : base(cancellationToken)
        {
            _pageInstance = pageInstance;
            _scale = scale;
            _viewport = viewport;
            _flags = flags;
            _backgroundColor = backgroundColor;
            _dispatcher = dispatcher;
        }

        protected override unsafe PdfBitmap OnExecute(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _bitmap = fpdfview.FPDFBitmapCreateEx(
                    (int)_viewport.Size.Width,
                    (int)_viewport.Size.Height,
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
                        (int)_viewport.Size.Width,
                        (int)_viewport.Size.Height,
                        _backgroundColor.Value.ToPixel<Argb32>().Argb);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                using var clipping = new FS_RECTF_
                {
                    Left = 0,
                    Right = _viewport.Size.Width,
                    Bottom = 0,
                    Top = _viewport.Size.Height
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
                    E = -_viewport.X,
                    F = -_viewport.Y
                };

                fpdfview.FPDF_RenderPageBitmapWithMatrix(_bitmap, _pageInstance, matrix, clipping, (int)_flags);

                cancellationToken.ThrowIfCancellationRequested();

                var scan0 = fpdfview.FPDFBitmapGetBuffer(_bitmap);

                var image = Image.WrapMemory<Bgra32>(
                        scan0.ToPointer(),
                        (int)_viewport.Size.Width,
                        (int)_viewport.Size.Height);

                return new PdfBitmap(_bitmap, image, _dispatcher, _scale, _viewport);
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