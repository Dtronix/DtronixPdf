using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
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

        public SizeF Size { get; private set; }

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

            var page = new PdfPage(dispatcher, documentInstance, loadPageResult);

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

        public Task<PdfBitmap> Render(RenderFlags flags)
        {
            return Render(flags, 1);
        }

        public Task<PdfBitmap> Render(RenderFlags flags, float scale)
        {
            return Render(flags, scale, (0, 0, 0, 0));
        }


        public Task<PdfBitmap> Render(RenderFlags flags, float scale,
            (float left, float top, float right, float bottom) clip)
        {
            return Render(flags, scale, clip, null);
        }

        public async Task<PdfBitmap> Render(
            RenderFlags flags,
            float scale,
            (float left, float top, float right, float bottom) clip,
            Color? backgroundColor)
        {
            return await _dispatcher.QueueWithResult(
                new RenderPageAction(_dispatcher, _pageInstance, scale, clip, flags, backgroundColor));
        }

        public async ValueTask DisposeAsync()
        {
            await _dispatcher.QueueForCompletion(() => { fpdfview.FPDF_ClosePage(_pageInstance); });
        }
    }
}