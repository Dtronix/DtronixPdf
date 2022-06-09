using System;
using System.IO;
using System.Runtime.InteropServices;
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

        public static Task<PdfDocument> Load(
            string path,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Load(path, password, PDFiumCoreManager.Default, cancellationToken);
        }

        public static async Task<PdfDocument> Load(
            Stream pdfStream,
            string password,
            CancellationToken cancellationToken = default)
        {
            await PDFiumCoreManager.Initialize();

            using var ms = new MemoryStream();
            await pdfStream.CopyToAsync(ms);

            return await Load(ms.ToArray(), password, PDFiumCoreManager.Default, cancellationToken);
        }

        public static async Task<PdfDocument> Load(
            byte[] fileBytes,
            string password,
            PDFiumCoreManager manager,
            CancellationToken cancellationToken = default)
        {
            await PDFiumCoreManager.Initialize();

            var ptr = Marshal.AllocHGlobal(fileBytes.Length);
            Marshal.Copy(fileBytes, 0, ptr, fileBytes.Length);

            return await Load(manager, _ =>
            {
                var document = fpdfview.FPDF_LoadMemDocument(ptr, fileBytes.Length, password);
                return (document, fpdfview.FPDF_GetPageCount(document));
            }, cancellationToken);
        }

        public static async Task<PdfDocument> Load(
            string path,
            string password,
            PDFiumCoreManager manager,
            CancellationToken cancellationToken = default)
        {
            await PDFiumCoreManager.Initialize();

            return await Load(manager, _ =>
            {
                var document = fpdfview.FPDF_LoadDocument(path, password);
                return (document, fpdfview.FPDF_GetPageCount(document));
            }, cancellationToken);
        }

        private static async Task<PdfDocument> Load(
            PDFiumCoreManager manager,
            Func<CancellationToken, (FpdfDocumentT Document, int Pages)> documentInitiator,
            CancellationToken cancellationToken = default)
        {
            await PDFiumCoreManager.Initialize();
            var result = await manager.Dispatcher.QueueResult(documentInitiator, cancellationToken: cancellationToken);
            if (result.Document == null)
                return null;

            var pdfDocument = new PdfDocument(manager, result.Document)
            {
                Pages = result.Pages,
            };

            manager.AddDocument(pdfDocument);

            return pdfDocument;
        }

        public static Task<PdfDocument> Create()
        {
            return Create(PDFiumCoreManager.Default);
        }

        public static async Task<PdfDocument> Create(PDFiumCoreManager manager)
        {
            var result = await manager.Dispatcher.QueueResult(_ => fpdf_edit.FPDF_CreateNewDocument());

            if (result == null)
                return null;

            return new PdfDocument(manager, result);
        }

        public Task<PdfPage> GetPage(int pageIndex)
        {
            return PdfPage.Create(_manager.Dispatcher, _documentInstance, pageIndex);
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
        public Task<bool> ImportPages(PdfDocument document, string pageRange, int insertIndex)
        {
            return _manager.Dispatcher.QueueResult(_ =>
            {
                var result = fpdf_ppo.FPDF_ImportPages(_documentInstance, document._documentInstance, pageRange, insertIndex);
                return result == 1;
            });
        }

        /// <summary>
        /// Extracts the specified page range into a new PdfDocument.
        /// </summary>
        /// <param name="pageRange">Pages are 1 based. Pages are separated by commas. Such as "1,3,5-7".</param>
        /// <returns>New document with the specified pages.</returns>
        public async Task<PdfDocument> ExtractPages(string pageRange)
        {
            var newDocument = await Create();
            await newDocument.ImportPages(this, pageRange, 0);

            return newDocument;
        }

        /// <summary>
        /// Deletes the specified page from the document.
        /// </summary>
        /// <param name="pageIndex">0 based index.</param>
        /// <returns>True on success, false on failure.</returns>
        public Task DeletePage(int pageIndex)
        {
            return _manager.Dispatcher.Queue(() => fpdf_edit.FPDFPageDelete(_documentInstance, pageIndex));
            
        }

        /// <summary>
        /// Saves the current document to the specified file path.
        /// </summary>
        /// <param name="path">Path to save the PdfDocument.</param>
        /// <returns>True on success, false on failure.</returns>
        public async Task<bool> Save(string path)
        {
            await using var fs = new FileStream(path, FileMode.Create);

            return await Save(fs);
        }

        /// <summary>
        /// Saves the current document to the passed stream.
        /// </summary>
        /// <param name="stream">Destination stream to write the PdfDocument.</param>
        /// <returns>True on success, false on failure.</returns>
        public async Task<bool> Save(Stream stream)
        {
            var writer = new PdfFileWriteCopyStream(stream);
            /*
             Flags
            #define FPDF_INCREMENTAL 1
            #define FPDF_NO_INCREMENTAL 2
            #define FPDF_REMOVE_SECURITY 3
             */

            var result = await _manager.Dispatcher.QueueResult(_ =>
                fpdf_save.FPDF_SaveAsCopy(_documentInstance, writer, 1));

            return result == 1;
        }

        public async ValueTask DisposeAsync()
        {
            await _manager.Dispatcher.Queue(() =>
            {
                fpdfview.FPDF_CloseDocument(_documentInstance);
            });

            _manager.RemoveDocument(this);
        }
    }
}