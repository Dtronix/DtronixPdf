﻿using System;
using System.Diagnostics;
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

        internal readonly PdfActionSynchronizer Synchronizer;

        private bool _isDisposed = false;
        private IntPtr? _documentPointer;

        public int Pages { get; private set; }

        private PdfDocument(PdfActionSynchronizer synchronizer, FpdfDocumentT instance)
        {
            Synchronizer = synchronizer;
            Instance = instance;
            PdfiumCoreManager.Default.AddDocument(this);
        }

        public static PdfDocument Load(
            string path,
            string password,
            CancellationToken cancellationToken = default)
        {
            PdfiumCoreManager.Initialize();

            var synchronizer = new PdfActionSynchronizer();

            var document = synchronizer.SyncExec(() => fpdfview.FPDF_LoadDocument(path, password));
            var pages = synchronizer.SyncExec(() => fpdfview.FPDF_GetPageCount(document));

            if (document == null)
                return null;

            var pdfDocument = new PdfDocument(synchronizer, document) { Pages = pages, };

            return pdfDocument;

        }

        public static unsafe PdfDocument Load(
            Stream stream,
            string password,
            CancellationToken cancellationToken = default)
        {
            var synchronizer = new PdfActionSynchronizer();

            var length = (int)stream.Length;

            var ptr = NativeMemory.Alloc((nuint)length);

            Span<byte> ptrSpan = new Span<byte>(ptr, length);
            var pointer = new IntPtr(ptr);
            var readLength = 0;

            // Copy the data to the memory.
            while ((readLength = stream.Read(ptrSpan)) > 0)
                ptrSpan = ptrSpan.Slice(readLength);

            PdfiumCoreManager.Initialize();

            int pages = -1;
            var result = synchronizer.SyncExec(() =>
            {
                var document = fpdfview.FPDF_LoadMemDocument(pointer, length, password);
                pages = fpdfview.FPDF_GetPageCount(document);
                return document;
            });

            if (result == null)
                return null;

            var pdfDocument = new PdfDocument(synchronizer, result)
            {
                Pages = pages,
                _documentPointer = pointer
            };

            return pdfDocument;
        }

        public static PdfDocument Create()
        {
            var synchronizer = new PdfActionSynchronizer();

            var result = synchronizer.SyncExec(fpdf_edit.FPDF_CreateNewDocument);

            if (result == null)
                return null;

            return new PdfDocument(synchronizer, result);
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
            return Synchronizer.SyncExec(() =>
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
            Synchronizer.SyncExec(() => fpdf_edit.FPDFPageDelete(Instance, pageIndex));
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

            var result = Synchronizer.SyncExec(() => fpdf_save.FPDF_SaveAsCopy(Instance, writer, 1));

            return result == 1;
        }

        public unsafe void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            Synchronizer.SyncExec(() => fpdfview.FPDF_CloseDocument(Instance));

            PdfiumCoreManager.Default.RemoveDocument(this);

            // Free the native memory.
            if (_documentPointer != null)
            {
                NativeMemory.Free(_documentPointer.Value.ToPointer());
                _documentPointer = null;
            }
        }
    }
}
