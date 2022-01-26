using System;
using System.Threading;
using DtronixPdf.Dispatcher;
using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DtronixPdf.Actions
{
    internal class RenderPageAction : ThreadMessagePumpAction<PdfBitmap>
    {
        public readonly FpdfPageT _pageInstance;
        private readonly float _scale;
        private readonly RectangleF _viewport;
        private readonly RenderFlags _flags;
        private readonly Color? _backgroundColor;
        private readonly bool _includeAlpha;
        private readonly CancellationToken _cancellationToken;
        private readonly ThreadDispatcher _dispatcher;
        private FpdfBitmapT _bitmap;

        public RenderPageAction(ThreadDispatcher dispatcher,
            FpdfPageT pageInstance,
            float scale,
            RectangleF viewport,
            RenderFlags flags,
            Color? backgroundColor,
            bool includeAlpha,
            CancellationToken cancellationToken)
        {
            _pageInstance = pageInstance;
            _scale = scale;
            _viewport = viewport;
            _flags = flags;
            _backgroundColor = backgroundColor;
            _includeAlpha = includeAlpha;
            _cancellationToken = cancellationToken;
            _dispatcher = dispatcher;
        }

        protected override unsafe PdfBitmap OnExecute()
        {
            try
            {
                _cancellationToken.ThrowIfCancellationRequested();

                _bitmap = fpdfview.FPDFBitmapCreateEx(
                    (int)_viewport.Size.Width,
                    (int)_viewport.Size.Height,
                    (int)(_includeAlpha ? FPDFBitmapFormat.BGRA : FPDFBitmapFormat.BGR),
                    IntPtr.Zero,
                    0);

                if (_bitmap == null)
                    throw new Exception("failed to create a bitmap object");

                _cancellationToken.ThrowIfCancellationRequested();
                if (_backgroundColor.HasValue)
                {
                    fpdfview.FPDFBitmapFillRect(
                        _bitmap,
                        0, 
                        0, 
                        (int)_viewport.Size.Width,
                        (int)_viewport.Size.Height,
                        _backgroundColor.Value.ToPixel<Argb32>().Argb);

                    _cancellationToken.ThrowIfCancellationRequested();
                }

                // |          | a b 0 |
                // | matrix = | c d 0 |
                // |          | e f 1 |
                using var matrix = new FS_MATRIX_();
                using var clipping = new FS_RECTF_();

                matrix.A = _scale;
                matrix.B = 0;
                matrix.C = 0;
                matrix.D = _scale;
                matrix.E = -_viewport.X;
                matrix.F = -_viewport.Y;

                clipping.Left = 0;
                clipping.Right = _viewport.Size.Width;
                clipping.Bottom = 0;
                clipping.Top = _viewport.Size.Height;

                fpdfview.FPDF_RenderPageBitmapWithMatrix(_bitmap, _pageInstance, matrix, clipping, (int)_flags);

                // Cancellation check;
                _cancellationToken.ThrowIfCancellationRequested();
                var scan0 = fpdfview.FPDFBitmapGetBuffer(_bitmap);

                var image = _includeAlpha
                    ? (Image)Image.WrapMemory<Bgra32>(
                        scan0.ToPointer(),
                        (int)_viewport.Size.Width,
                        (int)_viewport.Size.Height)
                    : Image.WrapMemory<Bgr24>(
                        scan0.ToPointer(),
                        (int)_viewport.Size.Width,
                        (int)_viewport.Size.Height);
                
                return new PdfBitmap(_bitmap, image, _dispatcher, _scale, _viewport);
            }
            catch (OperationCanceledException)
            {
                fpdfview.FPDFBitmapDestroy(_bitmap);
                throw;
            }
            catch (Exception ex)
            {
                fpdfview.FPDFBitmapDestroy(_bitmap);
                throw new Exception("Error rendering page. Check inner exception.", ex);
            }
        }
    }
}