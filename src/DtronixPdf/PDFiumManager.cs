using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtronixPdf.Dispatcher;
using PDFiumCore;

namespace DtronixPdf
{

    public class PDFiumCoreManager
    {
        private static bool IsInitialized;

        private static PDFiumCoreManager _managerDefaultInstance;
        public static PDFiumCoreManager Default => _managerDefaultInstance ??= new PDFiumCoreManager();

        public readonly ThreadDispatcher Dispatcher;

        private readonly List<PdfDocument> LoadedDocuments = new ();

        private static readonly ConcurrentBag<PDFiumCoreManager> LoadedManagers = new ();

        private PDFiumCoreManager()
        {
            LoadedManagers.Add(this);

            Dispatcher = new ThreadDispatcher();
            Dispatcher.Start();
        }

        /// <summary>
        /// Initialized the PDFiumCore library.
        /// </summary>
        /// <returns></returns>
        private static Task Initialize()
        {
            if (IsInitialized)
                return Task.CompletedTask;

            IsInitialized = true;

            // Initialize the library.
            return Default.Dispatcher.QueueForCompletion(fpdfview.FPDF_InitLibrary);
        }

        private static Task Destroy()
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
            return Default.Dispatcher.QueueForCompletion(fpdfview.FPDF_DestroyLibrary);
        }

        internal async Task AddDocument(PdfDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (!IsInitialized)
                await Initialize();

            lock (LoadedDocuments)
            {
                LoadedDocuments.Add(document);
            }
        }

        internal async Task RemoveDocument(PdfDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (!IsInitialized)
                await Initialize();

            lock (LoadedDocuments)
            {
                LoadedDocuments.Remove(document);
            }
        }
    }
}
