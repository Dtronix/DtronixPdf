using System;
using System.Runtime.Intrinsics;
using System.Threading;
using PDFiumCore;

namespace DtronixPdf
{
    public partial class PdfPage : IDisposable
    {
        internal readonly PdfDocument Document;
        private readonly FpdfPageT _pageInstance;
        private bool _isDisposed = false;

        public float Width { get; private set; }
        public float Height { get; private set; }

        public int InitialIndex { get; private set; }

        internal FpdfPageT PageInstance => _pageInstance;

        private PdfPage(PdfDocument document, FpdfPageT pageInstance)
        {
            Document = document;
            _pageInstance = pageInstance;
        }

        internal static PdfPage Create(
            PdfDocument document,
            int pageIndex)
        {
            var loadPageResult = PdfiumManager.Default.Synchronizer.SyncExec(() => fpdfview.FPDF_LoadPage(document.Instance, pageIndex));
            if (loadPageResult == null)
                throw new Exception($"Failed to open page for page index {pageIndex}.");

            var page = new PdfPage(document, loadPageResult)
            {
                InitialIndex = pageIndex
            };

            var getPageSizeResult = PdfiumManager.Default.Synchronizer.SyncExec(() =>
            {
                var size = new FS_SIZEF_();

                var result = fpdfview.FPDF_GetPageSizeByIndexF(document.Instance, pageIndex, size);

                return result == 0 ? null : size;
            });

            if (getPageSizeResult == null)
                throw new Exception($"Could not retrieve page size for page index {pageIndex}.");
            page.Width = getPageSizeResult.Width;
            page.Height = getPageSizeResult.Height;

            return page;
        }

        public PdfBitmap Render(float scale, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            var config = new PdfPageRenderConfig()
            {
                Scale = scale,
                Viewport = Vector128.Create(0, 0, Width * scale, Height * scale),
                CancellationToken = cancellationToken,
            };

            return Render(config);
        }

        public PdfBitmap Render(PdfPageRenderConfig config)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            FpdfBitmapT bitmap = null;

            var viewportHeight = config.Viewport.GetHeight();
            var viewportHeightInt = (int)viewportHeight;
            var viewportWidth = config.Viewport.GetWidth();
            var viewportWidthInt = (int)viewportWidth;



            try
            {
                config.CancellationToken.ThrowIfCancellationRequested();

                bitmap = PdfiumManager.Default.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapCreateEx(
                    viewportWidthInt,
                    viewportHeightInt,
                    (int)FPDFBitmapFormat.BGRA,
                    IntPtr.Zero,
                    0));

                if (bitmap == null)
                    throw new Exception("failed to create a bitmap object");

                config.CancellationToken.ThrowIfCancellationRequested();

                if (config.BackgroundColor.HasValue)
                {
                    PdfiumManager.Default.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapFillRect(
                        bitmap,
                        0,
                        0,
                        viewportWidthInt,
                        viewportHeightInt,
                        config.BackgroundColor.Value));

                    config.CancellationToken.ThrowIfCancellationRequested();
                }

                using var clipping = new FS_RECTF_
                {
                    Left = 0,
                    Right = viewportWidth,
                    Bottom = 0,
                    Top = viewportHeight
                };

                // |          | a b 0 |
                // | matrix = | c d 0 |
                // |          | e f 1 |
                using var matrix = new FS_MATRIX_
                {
                    A = config.Scale,
                    B = 0,
                    C = 0,
                    D = config.Scale,
                    E = config.OffsetX,
                    F = config.OffsetY
                };

                PdfiumManager.Default.Synchronizer.SyncExec(() =>
                    fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, _pageInstance, matrix, clipping,
                        (int)config.Flags));

                config.CancellationToken.ThrowIfCancellationRequested();

                return new PdfBitmap(bitmap, config.Scale, config.Viewport);
            }
            catch (OperationCanceledException)
            {
                if (bitmap != null)
                    PdfiumManager.Default.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapDestroy(bitmap));
                throw;
            }
            catch (Exception ex)
            {
                if (bitmap != null)
                    PdfiumManager.Default.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapDestroy(bitmap));

                throw new Exception("Error rendering page. Check inner exception.", ex);
            }
            finally
            {

            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            PdfiumManager.Default.Synchronizer.SyncExec(() => fpdfview.FPDF_ClosePage(PageInstance));
        }
    }
}
