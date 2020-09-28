using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DtronixPdf.Actions;
using DtronixPdf.Renderer;
using DtronixPdf.Renderer.Dispatcher;
using PDFiumCore;


namespace DtronixPdf
{
    public class PdfDocument : IAsyncDisposable
    {
        private readonly FpdfDocumentT _documentInstance;

        private static readonly ThreadDispatcher Dispatcher;

        static PdfDocument()
        {
            Dispatcher = new ThreadDispatcher();
            Dispatcher.Start();

            // Initialize the library.
            Dispatcher.QueueForCompletion(fpdfview.FPDF_InitLibrary);
        }

        private PdfDocument(FpdfDocumentT documentInstance)
        {
            _documentInstance = documentInstance;
        }

        public static async Task<PdfDocument> Load(string path, string password)
        {
            var result = await Dispatcher.QueueWithResult(() =>
                fpdfview.FPDF_LoadDocument(path, password));

            if (result == null)
                return null;

            var renderer = new PdfDocument(result);
            return renderer;
        }

        public static async Task<PdfDocument> Create()
        {
            var result = await Dispatcher.QueueWithResult(fpdf_edit.FPDF_CreateNewDocument);

            if (result == null)
                return null;

            return new PdfDocument(result);
        }

        public Task<PdfPage> GetPage(int pageIndex)
        {
            return PdfPage.Create(Dispatcher, _documentInstance, pageIndex);
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
            return await Dispatcher.QueueForCompletion(() =>
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
            return Dispatcher.QueueForCompletion(() => fpdf_edit.FPDFPageDelete(_documentInstance, pageIndex));
            
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

            var result = await Dispatcher.QueueForCompletion(() =>
                fpdf_save.FPDF_SaveAsCopy(_documentInstance, writer, 1));

            return result;
        }

        public async ValueTask DisposeAsync()
        {
            await Dispatcher.QueueForCompletion(() =>
                fpdfview.FPDF_CloseDocument(_documentInstance));

            Dispatcher.Stop();
        }
    }
}