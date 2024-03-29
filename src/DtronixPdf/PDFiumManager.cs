﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PDFiumCore;

namespace DtronixPdf
{

    public class PdfiumManager
    {
        private static bool IsInitialized;

        private static PdfiumManager _managerDefaultInstance;
        public static PdfiumManager Default => _managerDefaultInstance ??= new PdfiumManager();

        private readonly PdfActionSynchronizer _synchronizer;

        private readonly List<PdfDocument> LoadedDocuments = new ();

        private static readonly ConcurrentBag<PdfiumManager> LoadedManagers = new ();

        private PdfiumManager()
        {
            LoadedManagers.Add(this);

            _synchronizer = new PdfActionSynchronizer();
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
            Default._synchronizer.SyncExec(fpdfview.FPDF_InitLibrary);
        }

        public static void Unload()
        {
            if (!IsInitialized)
                return;

            foreach (var pdfiumCoreManager in LoadedManagers)
            {
                if (pdfiumCoreManager.LoadedDocuments.Count > 0)
                    throw new InvalidOperationException("Can't destroy loaded library since it is still in use by PdfDocument(s)");
            }

            IsInitialized = false;

            Default._synchronizer.SyncExec(fpdfview.FPDF_DestroyLibrary);
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
