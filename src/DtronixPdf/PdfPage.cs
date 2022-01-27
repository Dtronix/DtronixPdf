using System;
using System.Threading;
using System.Threading.Tasks;
using DtronixPdf.Actions;
using DtronixPdf.Dispatcher;
using DtronixPdf.Dispatcher.Actions;
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
            var loadPageResult = await dispatcher.QueueWithResult(() => fpdfview.FPDF_LoadPage(documentInstance, pageIndex));
            if (loadPageResult == null)
                throw new Exception($"Failed to open page for page index {pageIndex}.");

            var page = new PdfPage(dispatcher, documentInstance, loadPageResult)
            {
                InitialIndex = pageIndex
            };

            var getPageSizeResult = await dispatcher.QueueWithResult(() =>
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
            RenderFlags flags,
            float scale,
            RectangleF viewport,
            bool alpha,
            Color? backgroundColor,
            CancellationToken cancellationToken,
            DispatcherPriority priority)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            if (viewport.IsEmpty)
                throw new ArgumentException("Viewport is empty", nameof(viewport));

            return await _dispatcher.QueueWithResult(
                new RenderPageAction(
                    _dispatcher,
                    _pageInstance,
                    scale,
                    viewport,
                    flags,
                    backgroundColor,
                    alpha),
                priority, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            await _dispatcher.QueueForCompletion(new SimpleMessagePumpAction(() =>
            {
                fpdfview.FPDF_ClosePage(_pageInstance);
            }));
        }
    }
}