using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PDFiumCore;

namespace DtronixPdf
{

    public class PdfiumManager
    {
        private static bool _isInitialized;

        private static PdfiumManager _managerDefaultInstance;
        public static PdfiumManager Default => _managerDefaultInstance ??= new PdfiumManager();

        /// <summary>
        /// Gets the <see cref="PdfActionSynchronizer"/> instance used to synchronize actions in a PDF document.
        /// </summary>
        public PdfActionSynchronizer Synchronizer { get; }

        private readonly ConcurrentDictionary<PdfDocument, PdfDocument> _loadedDocuments = new ();

        private static readonly ConcurrentBag<PdfiumManager> _loadedManagers = new ();

        private PdfiumManager()
        {
            _loadedManagers.Add(this);

            Synchronizer = new PdfActionSynchronizer();
        }

        /// <summary>
        /// Initialized the PDFiumCore library.
        /// </summary>
        /// <returns></returns>
        internal static void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            // Initialize the library.
            Default.Synchronizer.SyncExec(fpdfview.FPDF_InitLibrary);
        }

        public static void Unload()
        {
            if (!_isInitialized)
                return;

            foreach (var pdfiumCoreManager in _loadedManagers)
            {
                if (pdfiumCoreManager._loadedDocuments.Count > 0)
                    throw new InvalidOperationException("Can't destroy loaded library since it is still in use by PdfDocument(s)");
            }

            _isInitialized = false;

            Default.Synchronizer.SyncExec(fpdfview.FPDF_DestroyLibrary);
        }

        internal void AddDocument(PdfDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            lock (_loadedDocuments)
            {
                _loadedDocuments.TryAdd(document, document);
            }
        }

        internal void RemoveDocument(PdfDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            lock (_loadedDocuments)
            {
                _loadedDocuments.Remove(document, out _);
            }
        }
    }
}
