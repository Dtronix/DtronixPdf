using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PDFiumCore;


namespace DtronixPdf
{
    public class PdfDocument : IDisposable
    {
        internal readonly FpdfDocumentT Instance;

        private readonly PdfiumCoreManager Manager;
        internal readonly PdfThreadDispatcher Dispatcher;

        private bool _isDisposed = false;
        private static IntPtr? _documentPointer;

        public int Pages { get; private set; }

        private PdfDocument(PdfiumCoreManager manager, FpdfDocumentT instance)
        {
            Dispatcher = manager.Dispatcher;
            Manager = manager;
            Instance = instance;
        }

        public static async Task<PdfDocument> LoadAsync(
            string path,
            string password,
            CancellationToken cancellationToken = default)
        {
            return await LoadAsync(path, password, PdfiumCoreManager.Default, cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task<PdfDocument> LoadAsync(
            string path,
            string password,
            PdfiumCoreManager manager,
            CancellationToken cancellationToken = default)
        {
            await PdfiumCoreManager.InitializeAsync().ConfigureAwait(false);

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

        public static PdfDocument Load(
            string path,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Load(path, password, PdfiumCoreManager.Default, cancellationToken);
        }

        public static PdfDocument Load(
            string path,
            string password,
            PdfiumCoreManager manager,
            CancellationToken cancellationToken = default)
        {
            PdfiumCoreManager.Initialize();

            try
            {
                manager.Dispatcher.Semaphore.Wait(cancellationToken);

                var document = fpdfview.FPDF_LoadDocument(path, password);
                var pages = fpdfview.FPDF_GetPageCount(document);

                if (document == null)
                    return null;

                var pdfDocument = new PdfDocument(manager, document)
                {
                    Pages = pages,
                };

                manager.AddDocument(pdfDocument);

                return pdfDocument;

            }
            finally
            {
                manager.Dispatcher.Semaphore.Release();
            }
        }

        public static PdfDocument Load(
            Stream stream,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Load(stream, password, PdfiumCoreManager.Default, cancellationToken);
        }


        public static unsafe PdfDocument Load(
            Stream stream,
            string password,
            PdfiumCoreManager manager,
            CancellationToken cancellationToken = default)
        {
            var length = (int)stream.Length;

            var ptr = NativeMemory.Alloc((nuint)length);

            Span<byte> ptrSpan = new Span<byte>(ptr, length);
            _documentPointer = new IntPtr(ptr);
            var readLength = 0;

            // Copy the data to the memory.
            while ((readLength = stream.Read(ptrSpan)) > 0)
                ptrSpan = ptrSpan.Slice(readLength);

            PdfiumCoreManager.Initialize();

            int pages = -1;
            var result = manager.Dispatcher.SyncExec(() =>
            {
                var document = fpdfview.FPDF_LoadMemDocument(_documentPointer.Value, length, password);
                pages = fpdfview.FPDF_GetPageCount(document);
                return document;
            });

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
            return await CreateAsync(PdfiumCoreManager.Default).ConfigureAwait(false);
        }

        public static async Task<PdfDocument> CreateAsync(PdfiumCoreManager manager)
        {
            var result = await manager.Dispatcher.QueueResult(_ => fpdf_edit.FPDF_CreateNewDocument())
                .ConfigureAwait(false);

            if (result == null)
                return null;

            return new PdfDocument(manager, result);
        }

        public static PdfDocument Create()
        {
            return Create(PdfiumCoreManager.Default);
        }

        public static PdfDocument Create(PdfiumCoreManager manager)
        {
            var result = manager.Dispatcher.SyncExec(fpdf_edit.FPDF_CreateNewDocument);

            if (result == null)
                return null;

            return new PdfDocument(manager, result);
        }



        public async Task<PdfPage> GetPageAsync(int pageIndex)
        {
            return await PdfPage.CreateAsync(this, pageIndex)
                .ConfigureAwait(false);
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
        public async Task<bool> ImportPagesAsync(PdfDocument document, string pageRange, int insertIndex)
        {
            return await Dispatcher.QueueResult(_ =>
            {
                var result = fpdf_ppo.FPDF_ImportPages(Instance, document.Instance, pageRange, insertIndex);
                return result == 1;
            }).ConfigureAwait(false);
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
            return Dispatcher.SyncExec(() =>
                fpdf_ppo.FPDF_ImportPages(Instance, document.Instance, pageRange, insertIndex) == 1);
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
        public async Task DeletePageAsync(int pageIndex)
        {
            await Dispatcher
                .Queue(() => fpdf_edit.FPDFPageDelete(Instance, pageIndex))
                .ConfigureAwait(true);
            
        }

        /// <summary>
        /// Deletes the specified page from the document.
        /// </summary>
        /// <param name="pageIndex">0 based index.</param>
        /// <returns>True on success, false on failure.</returns>
        public void DeletePage(int pageIndex)
        {
            Dispatcher.SyncExec(() => fpdf_edit.FPDFPageDelete(Instance, pageIndex));
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

            var result = await Dispatcher.QueueResult(_ =>
                fpdf_save.FPDF_SaveAsCopy(Instance, writer, 1)).ConfigureAwait(false);

            return result == 1;
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

            var result = Dispatcher.SyncExec(() => fpdf_save.FPDF_SaveAsCopy(Instance, writer, 1));

            return result == 1;
        }

        public unsafe void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            
            Dispatcher.SyncExec(() => fpdfview.FPDF_CloseDocument(Instance));

            // Free the native memory.
            if (_documentPointer != null)
                NativeMemory.Free(_documentPointer.Value.ToPointer());
        }
    }
}
