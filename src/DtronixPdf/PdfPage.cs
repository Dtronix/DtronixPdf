using System;
using System.Threading;
using System.Threading.Tasks;
using DtronixCommon;
using DtronixCommon.Threading.Dispatcher;
using DtronixCommon.Threading.Dispatcher.Actions;
using DtronixPdf.Actions;
using PDFiumCore;

namespace DtronixPdf
{
    public partial class PdfPage : IAsyncDisposable
    {
        private readonly ThreadDispatcher _dispatcher;
        private readonly FpdfDocumentT _documentInstance;
        private readonly FpdfPageT _pageInstance;
        private bool _isDisposed = false;

        public ThreadDispatcher Dispatcher => _dispatcher;

        public float Width { get; private set; }
        public float Height { get; private set; }

        public int InitialIndex { get; private set; }

        internal FpdfPageT PageInstance => _pageInstance;

        private PdfPage(ThreadDispatcher dispatcher, FpdfDocumentT documentInstance, FpdfPageT pageInstance)
        {
            _dispatcher = dispatcher;
            _documentInstance = documentInstance;
            _pageInstance = pageInstance;
        }

        internal static async Task<PdfPage> CreateAsync(
            ThreadDispatcher dispatcher,
            FpdfDocumentT documentInstance,
            int pageIndex)
        {
            var loadPageResult = await dispatcher.QueueResult(_ => fpdfview.FPDF_LoadPage(documentInstance, pageIndex))
                .ConfigureAwait(false);
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
            }).ConfigureAwait(false);

            if (getPageSizeResult == null)
                throw new Exception($"Could not retrieve page size for page index {pageIndex}.");
            page.Width = getPageSizeResult.Width;
            page.Height = getPageSizeResult.Height;

            return page;
        }

        public async Task<PdfBitmap> RenderAsync(
            float scale,
            uint? argbBackground,
            RenderFlags flags = RenderFlags.RenderAnnotations,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            return await _dispatcher.QueueResult(
                new RenderPageAction(
                    _dispatcher,
                    PageInstance,
                    scale,
                    new Boundary(0,0, Width, Height),
                    flags,
                    argbBackground,
                    cancellationToken)).ConfigureAwait(false);
        }

        public async Task<PdfBitmap> RenderAsync(
            float scale,
            uint? argbBackground,
            Boundary viewport,
            RenderFlags flags = RenderFlags.RenderAnnotations,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            if (viewport.IsEmpty)
                throw new ArgumentException("Viewport is empty", nameof(viewport));

            return await _dispatcher.QueueResult(
                new RenderPageAction(
                    _dispatcher,
                    PageInstance,
                    scale,
                    viewport,
                    flags,
                    argbBackground,
                    cancellationToken)).ConfigureAwait(false);
        }

        public async Task<PdfBitmap> RenderAsync(RenderPageAction action)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));
            
            return await _dispatcher.QueueResult(action).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            await _dispatcher.Queue(new SimpleMessagePumpAction(() =>
            {
                fpdfview.FPDF_ClosePage(PageInstance);
            })).ConfigureAwait(false);
        }
    }
}
