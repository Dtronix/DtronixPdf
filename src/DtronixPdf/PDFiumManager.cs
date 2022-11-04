using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DtronixCommon.Threading.Dispatcher;
using PDFiumCore;

namespace DtronixPdf
{

    public class PdfiumCoreManager
    {
        private static bool IsInitialized;

        private static PdfiumCoreManager _managerDefaultInstance;
        public static PdfiumCoreManager Default => _managerDefaultInstance ??= new PdfiumCoreManager();

        public readonly PdfThreadDispatcher Dispatcher;

        private readonly List<PdfDocument> LoadedDocuments = new ();

        private static readonly ConcurrentBag<PdfiumCoreManager> LoadedManagers = new ();

        private PdfiumCoreManager()
        {
            LoadedManagers.Add(this);

            Dispatcher = new PdfThreadDispatcher(1);
            Dispatcher.Start();
        }

        /// <summary>
        /// Initialized the PDFiumCore library.
        /// </summary>
        /// <returns></returns>
        internal static Task InitializeAsync()
        {
            if (IsInitialized)
                return Task.CompletedTask;

            IsInitialized = true;
            // Initialize the library.
            return Default.Dispatcher.Queue(fpdfview.FPDF_InitLibrary);
        }

        /// <summary>
        /// Initialized the PDFiumCore library.
        /// </summary>
        /// <returns></returns>
        internal static void Initialize()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            // Initialize the library.
            Default.Dispatcher.SyncExec(fpdfview.FPDF_InitLibrary);
        }

        private static Task UnloadAsync()
        {
            if (!IsInitialized)
                return Task.CompletedTask;

            foreach (var pdfiumCoreManager in LoadedManagers)
            {
                if (pdfiumCoreManager.LoadedDocuments.Count > 0)
                    throw new InvalidOperationException("Can't destroy loaded library since it is still in use by PdfDocument(s)");
            }

            IsInitialized = false;

            // Initialize the library.
            return Default.Dispatcher.Queue(fpdfview.FPDF_DestroyLibrary);
        }

        private static void Unload()
        {
            if (!IsInitialized)
                return;

            foreach (var pdfiumCoreManager in LoadedManagers)
            {
                if (pdfiumCoreManager.LoadedDocuments.Count > 0)
                    throw new InvalidOperationException("Can't destroy loaded library since it is still in use by PdfDocument(s)");
            }

            IsInitialized = false;

            Default.Dispatcher.SyncExec(fpdfview.FPDF_DestroyLibrary);
        }

        internal void AddDocument(PdfDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            lock (LoadedDocuments)
            {
                LoadedDocuments.Add(document);
            }
        }

        internal void RemoveDocument(PdfDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            lock (LoadedDocuments)
            {
                LoadedDocuments.Remove(document);
            }
        }
    }
}
