using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DtronixPdf.Actions;
using DtronixPdf.Renderer.Dispatcher;
using PDFiumCore;

namespace DtronixPdf
{
    public class PdfPage : IAsyncDisposable
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
            var loadPageResult = await dispatcher.QueueWithResult(() =>
                fpdfview.FPDF_LoadPage(documentInstance, pageIndex));
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
        /*
        public Task<PdfBitmap> Render(RenderFlags flags)
        {
            return Render(flags, 1);
        }

        public Task<PdfBitmap> Render(RenderFlags flags, float scale)
        {
            return Render(flags, scale, new Rectangle(0, 0, (int) (Size.Width * scale), (int) (Size.Height * scale)));
        }


        public Task<PdfBitmap> Render(RenderFlags flags, float scale, RectangleF viewport)
        {
            return Render(flags, scale, viewport, false, Color.White);
        }
        
        public Task<PdfBitmap> Render(
            RenderFlags flags,
            float scale,
            Viewport viewport,
            bool alpha,
            Color? backgroundColor)
        {
            var translatedRectangle = new RectangleF(
                (int) ((Size.Width / 2 - viewport.Size.Width / 2 + viewport.Origin.X) * scale + viewport.Size.Width / 2 * (scale - 1)),
                (int) ((Size.Height / 2 - viewport.Size.Height / 2 - viewport.Origin.Y) * scale + viewport.Size.Height / 2 * (scale - 1)),
                viewport.Size.Width,
                viewport.Size.Height);

            return Render(flags, scale, translatedRectangle, alpha, backgroundColor);
        }
        */

        public async Task<PdfBitmap> Render(
            RenderFlags flags,
            float scale,
            Viewport viewport,
            bool alpha,
            Color? backgroundColor,
            CancellationToken cancellationToken)
        {
            if(_isDisposed)
                throw new ObjectDisposedException(nameof(PdfPage));

            if(viewport.IsEmpty)
                throw new ArgumentException("Viewport is empty", nameof(viewport));

            return await _dispatcher.QueueWithResult(
                new RenderPageAction(_dispatcher, _pageInstance, scale, viewport, flags, backgroundColor, alpha, cancellationToken));
        }


        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            await _dispatcher.QueueForCompletion(() =>
            {
                fpdfview.FPDF_ClosePage(_pageInstance);
            });
        }
    }
}