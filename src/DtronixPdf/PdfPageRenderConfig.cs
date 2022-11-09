using System;
using System.Threading;
using DtronixCommon;
using PDFiumCore;

namespace DtronixPdf
{
    public record class PdfPageRenderConfig
    {
        public float Scale { get; init; }
        public Boundary Viewport { get; init; }

        public uint? BackgroundColor { get; init; }

        public float OffsetX { get; init; }

        public float OffsetY { get; init; }

        public RenderFlags Flags { get; init; } = RenderFlags.RenderAnnotations;

        public CancellationToken CancellationToken { get; init; }
    }
}
