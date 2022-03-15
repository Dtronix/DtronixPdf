using System;
using System.Threading;
using System.Threading.Tasks;
using DtronixCommon.Threading.Dispatcher;
using DtronixCommon.Threading.Dispatcher.Actions;
using DtronixPdf.Actions;
using PDFiumCore;
using SixLabors.ImageSharp;

namespace DtronixPdf
{
    public partial class PdfPage : IAsyncDisposable
    {
        private readonly ThreadDispatcher _dispatcher;
        private readonly FpdfDocumentT _documentInstance;
        private readonly FpdfPageT _pageInstance;
        private bool _isDisposed = false;

        public SizeF Size { get; private set; }

        public int InitialIndex { get; private set; }

        private PdfPage(ThreadDispatcher dispatcher, FpdfDocumentT documentInstance, FpdfPageT pageInstance)
        {
            _dispatcher = dispatcher;
            _documentInstance = documentInstance;
            _pageInstance = pageInstance;
        }

        internal static async Task<PdfPage> Create(
            ThreadDispatcher dispatcher,
            FpdfDocumentT documentInstance,
            int pageIndex)
        {
            var loadPageResult = await dispatcher.QueueResult(_ => fpdfview.FPDF_LoadPage(documentInstance, pageIndex));
            if (loadPageResult == null)
                throw new Exception($"Failed to open page for page index {pageIndex}.");

            var page = new PdfPage(dispatcher, documentInstance, loadPageResult)
            {
                InitialIndex = pageIndex
            };

            var getPageSizeResult = await dispatcher.QueueResult(_ =>
            {
                var size = new FS_SIZEF_();
                
                var result = fpdfview.FPDF_GetPageSizeByIndexF(documentInstance, pageIndex, size);

                return result == 0 ? null : size;
            });

            if (getPageSizeResult == null)
                throw new Exception($"Could not retrieve page size for page index {pageIndex}.");

            page.Size = new SizeF(getPageSizeResult.Width, getPageSizeResult.Height);

            return page;
        }

        public async Task<PdfBitmap> Render(
            float scale,
            Color? backgroundColor,
            RenderFlags flags = RenderFlags.RenderAnnotations,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            return await _dispatcher.QueueResult(
                new RenderPageAction(
                    _dispatcher,
                    _pageInstance,
                    scale,
                    new RectangleF(0,0, Size.Width, Size.Height),
                    flags,
                    backgroundColor,
                    cancellationToken));
        }

        public async Task<PdfBitmap> Render(
            float scale,
            Color? backgroundColor,
            RectangleF viewport,
            RenderFlags flags = RenderFlags.RenderAnnotations,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            if (viewport.IsEmpty)
                throw new ArgumentException("Viewport is empty", nameof(viewport));

            return await _dispatcher.QueueResult(
                new RenderPageAction(
                    _dispatcher,
                    _pageInstance,
                    scale,
                    viewport,
                    flags,
                    backgroundColor,
                    cancellationToken));
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            await _dispatcher.Queue(new SimpleMessagePumpAction(() =>
            {
                fpdfview.FPDF_ClosePage(_pageInstance);
            }));
        }
    }
}