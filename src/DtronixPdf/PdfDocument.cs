using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DtronixPdf.Actions;
using DtronixPdf.Dispatcher;
using PDFiumCore;


namespace DtronixPdf
{
    public class PdfDocument : IAsyncDisposable
    {
        private readonly FpdfDocumentT _documentInstance;

        private readonly PDFiumManager _manager;

        public int Pages { get; private set; }


        private PdfDocument(PDFiumManager manager, FpdfDocumentT documentInstance)
        {
            _manager = manager;
            _documentInstance = documentInstance;
        }

        public static Task<PdfDocument> Load(string path, string password)
        {
            return Load(path, password, PDFiumManager.Default);
        }

        public static async Task<PdfDocument> Load(
            string path, 
            string password,
            PDFiumManager manager)
        {

            int pages = -1;
            var result = await manager.Dispatcher.QueueWithResult(() =>
            {
                var document = fpdfview.FPDF_LoadDocument(path, password);
                pages = fpdfview.FPDF_GetPageCount(document);
                return document;
            });

            if (result == null)
                return null;

            var pdfDocument = new PdfDocument(manager, result)
            {
                Pages = pages,
            };

            return pdfDocument;
        }

        public static Task<PdfDocument> Create()
        {
            return Create(PDFiumManager.Default);
        }

        public static async Task<PdfDocument> Create(PDFiumManager manager)
        {
            var result = await manager.Dispatcher.QueueWithResult(fpdf_edit.FPDF_CreateNewDocument);

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
        public async Task<bool> ImportPages(PdfDocument document, string pageRange, int insertIndex)
        {
            return await _manager.Dispatcher.QueueForCompletion(() =>
                fpdf_ppo.FPDF_ImportPages(_documentInstance, document._documentInstance, pageRange, insertIndex));
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
        public Task<bool> DeletePage(int pageIndex)
        {
            return _manager.Dispatcher.QueueForCompletion(() => fpdf_edit.FPDFPageDelete(_documentInstance, pageIndex));
            
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

            var result = await _manager.Dispatcher.QueueForCompletion(() =>
                fpdf_save.FPDF_SaveAsCopy(_documentInstance, writer, 1));

            return result;
        }

        public async ValueTask DisposeAsync()
        {
            await _manager.Dispatcher.QueueForCompletion(() =>
            {
                fpdfview.FPDF_CloseDocument(_documentInstance);
            });
        }
    }
}