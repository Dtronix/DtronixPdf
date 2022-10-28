using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PDFiumCore;


namespace DtronixPdf
{
    public class PdfDocument : IAsyncDisposable
    {
        private readonly FpdfDocumentT _documentInstance;

        private readonly PDFiumCoreManager _manager;

        public int Pages { get; private set; }


        private PdfDocument(PDFiumCoreManager manager, FpdfDocumentT documentInstance)
        {
            _manager = manager;
            _documentInstance = documentInstance;
        }

        public static async Task<PdfDocument> LoadAsync(
            string path,
            string password,
            CancellationToken cancellationToken = default)
        {
            return await LoadAsync(path, password, PDFiumCoreManager.Default, cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task<PdfDocument> LoadAsync(
            string path,
            string password,
            PDFiumCoreManager manager,
            CancellationToken cancellationToken = default)
        {
            await PDFiumCoreManager.Initialize().ConfigureAwait(false);

            int pages = -1;
            var result = await manager.Dispatcher.QueueResult(_ =>
                {
                    var document = fpdfview.FPDF_LoadDocument(path, password);
                    pages = fpdfview.FPDF_GetPageCount(document);
                    return document;
                }, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (result == null)
                return null;

            var pdfDocument = new PdfDocument(manager, result)
            {
                Pages = pages,
            };

            manager.AddDocument(pdfDocument);

            return pdfDocument;
        }

        public static async Task<PdfDocument> CreateAsync()
        {
            return await CreateAsync(PDFiumCoreManager.Default).ConfigureAwait(false);
        }

        public static async Task<PdfDocument> CreateAsync(PDFiumCoreManager manager)
        {
            var result = await manager.Dispatcher.QueueResult(_ => fpdf_edit.FPDF_CreateNewDocument())
                .ConfigureAwait(false);

            if (result == null)
                return null;

            return new PdfDocument(manager, result);
        }

        public async Task<PdfPage> GetPageAsync(int pageIndex)
        {
            return await PdfPage.CreateAsync(_manager.Dispatcher, _documentInstance, pageIndex)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Imports pages from another PDF document.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="pageRange">
        /// Pages are 1 based. Pages are separated by commas. Such as "1,3,5-7".
        /// If null, all pages are imported.</param>
        /// <param name="insertIndex">Insertion index is 0 based.</param>
        /// <returns>True on success, false on failure.</returns>
        public async Task<bool> ImportPagesAsync(PdfDocument document, string pageRange, int insertIndex)
        {
            return await _manager.Dispatcher.QueueResult(_ =>
            {
                var result = fpdf_ppo.FPDF_ImportPages(_documentInstance, document._documentInstance, pageRange, insertIndex);
                return result == 1;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Extracts the specified page range into a new PdfDocument.
        /// </summary>
        /// <param name="pageRange">Pages are 1 based. Pages are separated by commas. Such as "1,3,5-7".</param>
        /// <returns>New document with the specified pages.</returns>
        public async Task<PdfDocument> ExtractPagesAsync(string pageRange)
        {
            var newDocument = await CreateAsync().ConfigureAwait(false);
            await newDocument.ImportPagesAsync(this, pageRange, 0).ConfigureAwait(false);

            return newDocument;
        }

        /// <summary>
        /// Deletes the specified page from the document.
        /// </summary>
        /// <param name="pageIndex">0 based index.</param>
        /// <returns>True on success, false on failure.</returns>
        public async Task DeletePageAsync(int pageIndex)
        {
            await _manager.Dispatcher
                .Queue(() => fpdf_edit.FPDFPageDelete(_documentInstance, pageIndex))
                .ConfigureAwait(true);
            
        }

        /// <summary>
        /// Saves the current document to the specified file path.
        /// </summary>
        /// <param name="path">Path to save the PdfDocument.</param>
        /// <returns>True on success, false on failure.</returns>
        public async Task<bool> SaveAsync(string path)
        {
            await using var fs = new FileStream(path, FileMode.Create);

            return await SaveAsync(fs).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves the current document to the passed stream.
        /// </summary>
        /// <param name="stream">Destination stream to write the PdfDocument.</param>
        /// <returns>True on success, false on failure.</returns>
        public async Task<bool> SaveAsync(Stream stream)
        {
            var writer = new PdfFileWriteCopyStream(stream);
            /*
             Flags
            #define FPDF_INCREMENTAL 1
            #define FPDF_NO_INCREMENTAL 2
            #define FPDF_REMOVE_SECURITY 3
             */

            var result = await _manager.Dispatcher.QueueResult(_ =>
                fpdf_save.FPDF_SaveAsCopy(_documentInstance, writer, 1)).ConfigureAwait(false);

            return result == 1;
        }

        public async ValueTask DisposeAsync()
        {
            await _manager.Dispatcher.Queue(() =>
            {
                fpdfview.FPDF_CloseDocument(_documentInstance);
            }).ConfigureAwait(false);

            _manager.RemoveDocument(this);
        }
    }
}
