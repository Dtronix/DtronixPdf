﻿using System;
using System.Runtime.Intrinsics;
using PDFiumCore;

namespace DtronixPdf
{
    public class PdfBitmap : IDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;

        public float Scale { get; }

        /// <summary>
        /// MinX, MinY, MaxX, MaxY
        /// </summary>
        public Vector128<float> Viewport { get; }

        public IntPtr Pointer { get; }

        public int Stride { get; }

        public int Width { get; }

        public int Height { get; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Only call within the synchronizer since dll calls are made.
        /// </summary>
        /// <param name="pdfBitmap"></param>
        /// <param name="scale"></param>
        /// <param name="viewport">MinX, MinY, MaxX, MaxY</param>
        internal PdfBitmap(
            FpdfBitmapT pdfBitmap,
            float scale,
            Vector128<float> viewport)
        {
            _pdfBitmap = pdfBitmap;
            Stride = fpdfview.FPDFBitmapGetStride(_pdfBitmap);
            Width = (int)viewport.GetWidth();
            Height = (int)viewport.GetHeight();
            Pointer = fpdfview.FPDFBitmapGetBuffer(_pdfBitmap);
            Scale = scale;
            Viewport = viewport;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            PdfiumManager.Default.Synchronizer.SyncExec(() => fpdfview.FPDFBitmapDestroy(_pdfBitmap));
        }
    }
}
