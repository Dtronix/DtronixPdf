﻿using System;
using DtronixCommon;
using PDFiumCore;

namespace DtronixPdf
{
    public class PdfBitmap : IDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;

        public float Scale { get; }

        public BoundaryF Viewport { get; }

        public IntPtr Pointer { get; }

        public int Stride { get; }

        public int Width { get; }

        public int Height { get; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Only call within the synchronizer since dll calls are made.
        /// </summary>
        /// <param name="pdfBitmap"></param>
        /// <param name="synchronizer"></param>
        /// <param name="scale"></param>
        /// <param name="viewport"></param>
        internal PdfBitmap(
            FpdfBitmapT pdfBitmap,
            float scale, 
            BoundaryF viewport)
        {
            _pdfBitmap = pdfBitmap;
            Stride = fpdfview.FPDFBitmapGetStride(_pdfBitmap);
            Width = (int)viewport.Width;
            Height = (int)viewport.Height;
            Pointer = fpdfview.FPDFBitmapGetBuffer(_pdfBitmap);
            Scale = scale;
            Viewport = viewport;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            PdfActionSync.Default.SyncExec(() => fpdfview.FPDFBitmapDestroy(_pdfBitmap));
        }
    }
}
