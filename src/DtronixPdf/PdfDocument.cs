using System;
using System.IO;
using System.Runtime.InteropServices;
using PDFiumCore;


namespace DtronixPdf
{
    public class PdfDocument : IDisposable
    {
        internal readonly FpdfDocumentT Instance;

        private bool _isDisposed = false;
        private IntPtr? _documentPointer;

        public int Pages { get; private set; }

        private PdfDocument(FpdfDocumentT instance)
        {
            Instance = instance;
            PdfiumManager.Default.AddDocument(this);
        }

        public static PdfDocument Load(string path, string password)
        {
            PdfiumManager.Initialize();

            var document = PdfActionSync.Default.SyncExec(
                static (path, password) => fpdfview.FPDF_LoadDocument(path, password),
                path, password);
            var pages = PdfActionSync.Default.SyncExec(
                static document => fpdfview.FPDF_GetPageCount(document),
                document);

            if (document == null)
                return null;

            var pdfDocument = new PdfDocument(document) { Pages = pages, };

            return pdfDocument;

        }

        public static unsafe PdfDocument Load(Stream stream, string password)
        {
            var length = (int)stream.Length;

            var ptr = NativeMemory.Alloc((nuint)length);

            Span<byte> ptrSpan = new Span<byte>(ptr, length);
            var pointer = new IntPtr(ptr);
            var readLength = 0;

            // Copy the data to the memory.
            while ((readLength = stream.Read(ptrSpan)) > 0)
                ptrSpan = ptrSpan.Slice(readLength);

            PdfiumManager.Initialize();

            int pages = -1;
            var result = PdfActionSync.Default.SyncExec(() =>
            {
                var document = fpdfview.FPDF_LoadMemDocument(pointer, length, password);
                pages = fpdfview.FPDF_GetPageCount(document);
                return document;
            });

            if (result == null)
                return null;

            var pdfDocument = new PdfDocument(result)
            {
                Pages = pages,
                _documentPointer = pointer
            };

            return pdfDocument;
        }

        public static PdfDocument Create()
        {
            var result = PdfActionSync.Default.SyncExec(fpdf_edit.FPDF_CreateNewDocument);

            if (result == null)
                return null;

            return new PdfDocument(result);
        }

        public PdfPage GetPage(int pageIndex)
        {
            return PdfPage.Create(this, pageIndex);
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
        public bool ImportPages(PdfDocument document, string pageRange, int insertIndex)
        {
            return PdfActionSync.Default.SyncExec(() =>
                fpdf_ppo.FPDF_ImportPages(Instance, document.Instance, pageRange, insertIndex) == 1);
        }

        /// <summary>
        /// Extracts the specified page range into a new PdfDocument.
        /// </summary>
        /// <param name="pageRange">Pages are 1 based. Pages are separated by commas. Such as "1,3,5-7".</param>
        /// <returns>New document with the specified pages.</returns>
        public PdfDocument ExtractPages(string pageRange)
        {
            var newDocument = Create();
            newDocument.ImportPages(this, pageRange, 0);

            return newDocument;
        }

        /// <summary>
        /// Deletes the specified page from the document.
        /// </summary>
        /// <param name="pageIndex">0 based index.</param>
        /// <returns>True on success, false on failure.</returns>
        public void DeletePage(int pageIndex)
        {
            PdfActionSync.Default.SyncExec(() => fpdf_edit.FPDFPageDelete(Instance, pageIndex));
        }


        /// <summary>
        /// Saves the current document to the specified file path.
        /// </summary>
        /// <param name="path">Path to save the PdfDocument.</param>
        /// <returns>True on success, false on failure.</returns>
        public bool Save(string path)
        {
            using var fs = new FileStream(path, FileMode.Create);
            return Save(fs);
        }

        /// <summary>
        /// Saves the current document to the passed stream.
        /// </summary>
        /// <param name="stream">Destination stream to write the PdfDocument.</param>
        /// <returns>True on success, false on failure.</returns>
        public bool Save(Stream stream)
        {
            var writer = new PdfFileWriteCopyStream(stream);
            /*
             Flags
            #define FPDF_INCREMENTAL 1
            #define FPDF_NO_INCREMENTAL 2
            #define FPDF_REMOVE_SECURITY 3
             */

            var result = PdfActionSync.Default.SyncExec(() => fpdf_save.FPDF_SaveAsCopy(Instance, writer, 1));

            return result == 1;
        }

        public unsafe void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            PdfActionSync.Default.SyncExec(() => fpdfview.FPDF_CloseDocument(Instance));

            PdfiumManager.Default.RemoveDocument(this);

            // Free the native memory.
            if (_documentPointer != null)
            {
                NativeMemory.Free(_documentPointer.Value.ToPointer());
                _documentPointer = null;
            }
        }
    }
}
