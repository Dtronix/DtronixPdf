using System;
using System.Reflection.Metadata;
using System.Threading;
using DtronixCommon;
using PDFiumCore;

namespace DtronixPdf
{
    public partial class PdfPage : IDisposable
    {
        internal readonly PdfDocument Document;
        internal readonly FpdfPageT PageInstance;
        private bool _isDisposed = false;

        public float Width { get; private set; }
        public float Height { get; private set; }

        public int InitialIndex { get; private set; }

        private PdfPage(PdfDocument document, FpdfPageT pageInstance)
        {
            Document = document;
            PageInstance = pageInstance;
        }

        internal static PdfPage Create(
            PdfDocument document,
            int pageIndex)
        {
            var loadPageResult = document.Synchronizer.SyncExec(() => fpdfview.FPDF_LoadPage(document.Instance, pageIndex));
            if (loadPageResult == null)
                throw new Exception($"Failed to open page for page index {pageIndex}.");

            var page = new PdfPage(document, loadPageResult)
            {
                InitialIndex = pageIndex
            };

            var getPageSizeResult = document.Synchronizer.SyncExec(() =>
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
                Viewport = new BoundaryF(0, 0, Width * scale, Height * scale),
                CancellationToken = cancellationToken,
            };

            return Render(config);
        }

        public PdfBitmap Render(PdfPageRenderConfig config)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            FpdfBitmapT bitmap = null;

            try
            {
                config.CancellationToken.ThrowIfCancellationRequested();

                bitmap = Document.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapCreateEx(
                    (int)config.Viewport.Width,
                    (int)config.Viewport.Height,
                    (int)FPDFBitmapFormat.BGRA,
                    IntPtr.Zero,
                    0));

                if (bitmap == null)
                    throw new Exception("failed to create a bitmap object");

                config.CancellationToken.ThrowIfCancellationRequested();

                if (config.BackgroundColor.HasValue)
                {
                    Document.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapFillRect(
                        bitmap,
                        0,
                        0,
                        (int)config.Viewport.Width,
                        (int)config.Viewport.Height,
                        config.BackgroundColor.Value));

                    config.CancellationToken.ThrowIfCancellationRequested();
                }

                using var clipping = new FS_RECTF_
                {
                    Left = 0,
                    Right = config.Viewport.Width,
                    Bottom = 0,
                    Top = config.Viewport.Height
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

                Document.Synchronizer.SyncExec(() =>
                    fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, PageInstance, matrix, clipping,
                        (int)config.Flags));

                config.CancellationToken.ThrowIfCancellationRequested();

                return new PdfBitmap(bitmap, Document.Synchronizer, config.Scale, config.Viewport);
            }
            catch (OperationCanceledException)
            {
                if (bitmap != null)
                    Document.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapDestroy(bitmap));
                throw;
            }
            catch (Exception ex)
            {
                if (bitmap != null)
                    Document.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapDestroy(bitmap));

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

            Document.Synchronizer.SyncExec(() => fpdfview.FPDF_ClosePage(PageInstance));
        }
    }
}
