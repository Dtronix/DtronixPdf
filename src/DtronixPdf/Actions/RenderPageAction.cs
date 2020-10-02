using System;
using System.Drawing;
using System.Drawing.Imaging;
using DtronixPdf.Renderer.Dispatcher;
using PDFiumCore;

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
        private readonly ThreadDispatcher _dispatcher;

        public RenderPageAction(
            ThreadDispatcher dispatcher,
            FpdfPageT pageInstance,
            float scale,
            RectangleF viewport,
            RenderFlags flags,
            Color? backgroundColor,
            bool includeAlpha)
        {
            _pageInstance = pageInstance;
            _scale = scale;
            _viewport = viewport;
            _flags = flags;
            _backgroundColor = backgroundColor;
            _includeAlpha = includeAlpha;
            _dispatcher = dispatcher;
        }

        protected override PdfBitmap OnExecute()
        {
            
            var bitmap = fpdfview.FPDFBitmapCreateEx(
                (int)_viewport.Width,
                (int)_viewport.Height,
                (int) (_includeAlpha ? FPDFBitmapFormat.BGRA : FPDFBitmapFormat.BGR) , 
                IntPtr.Zero, 
                0);

            if(_backgroundColor.HasValue)
                fpdfview.FPDFBitmapFillRect(
                    bitmap, 0, 0, (int)_viewport.Width, (int)_viewport.Height, (uint) _backgroundColor.Value.ToArgb());

            if (bitmap == null)
                throw new Exception("failed to create a bitmap object");

            try
            {
                // |          | a b 0 |
                // | matrix = | c d 0 |
                // |          | e f 1 |
                using var matrix = new FS_MATRIX_();
                using var clipping = new FS_RECTF_();

                matrix.A = _scale;
                matrix.B = 0;
                matrix.C = 0;
                matrix.D = _scale;
                matrix.E = -_viewport.Left;
                matrix.F = -_viewport.Top;

                clipping.Left = 0;
                clipping.Right = _viewport.Width;
                clipping.Bottom = 0;
                clipping.Top = _viewport.Height;

                fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, _pageInstance, matrix, clipping, (int) _flags);

                return new PdfBitmap(
                    bitmap,
                    (int)_viewport.Width,
                    (int)_viewport.Height,
                    _dispatcher,
                    _includeAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb,
                    _scale,
                    _viewport);
            }
            catch (Exception ex)
            {
                throw new Exception("error rendering page", ex);
            }
        }
    }
}