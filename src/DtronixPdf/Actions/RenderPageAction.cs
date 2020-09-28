using System;
using System.Drawing;
using DtronixPdf.Renderer.Dispatcher;
using PDFiumCore;

namespace DtronixPdf.Actions
{
    internal class RenderPageAction : ThreadMessagePumpAction<PdfBitmap>
    {
        public readonly FpdfPageT _pageInstance;
        private readonly float _scale;
        private readonly (float left, float top, float right, float bottom) _clip;
        private readonly RenderFlags _flags;
        private readonly Color? _backgroundColor;
        private readonly ThreadDispatcher _dispatcher;

        public RenderPageAction(
            ThreadDispatcher dispatcher,
            FpdfPageT pageInstance,
            float scale,
            (float left, float top, float right, float bottom) clip,
            RenderFlags flags,
            Color? backgroundColor)
        {
            _pageInstance = pageInstance;
            _scale = scale;
            _clip = clip;
            _flags = flags;
            _backgroundColor = backgroundColor;
            _dispatcher = dispatcher;
        }

        protected override PdfBitmap OnExecute()
        {
            var width = (int) ((fpdfview.FPDF_GetPageWidth(_pageInstance) * _scale));
            var height = (int) (fpdfview.FPDF_GetPageHeight(_pageInstance) * _scale);

            var bitmap = fpdfview.FPDFBitmapCreate(width, height, _backgroundColor.HasValue ? 0 : 1);

            if (_backgroundColor.HasValue)
            {
                fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, width, height, (uint) _backgroundColor.Value.ToArgb());
            }

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
                matrix.E = -_clip.left;
                matrix.F = -_clip.top;

                clipping.Left = 0;
                clipping.Right = width - _clip.right;
                clipping.Bottom = 0;
                clipping.Top = height - _clip.bottom;

                fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, _pageInstance, matrix, clipping, (int) _flags);

                return new PdfBitmap(bitmap, (int) (width - _clip.left - _clip.right), (int)(height - _clip.top - _clip.bottom), _dispatcher);
            }
            catch (Exception ex)
            {
                throw new Exception("error rendering page", ex);
            }
        }
    }
}