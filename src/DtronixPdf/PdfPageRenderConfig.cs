using System.Runtime.Intrinsics;
using System.Threading;
using PDFiumCore;

namespace DtronixPdf
{
    public record class PdfPageRenderConfig
    {
        public float Scale { get; init; }

        /// <summary>
        /// Viewport must be setup with MinX, MinY, MaxX, MaxY formatting.
        /// </summary>
        public Vector128<float> Viewport { get; init; }

        public uint? BackgroundColor { get; init; }

        public float OffsetX { get; init; }

        public float OffsetY { get; init; }

        public RenderFlags Flags { get; init; } = RenderFlags.RenderAnnotations;

        public CancellationToken CancellationToken { get; init; }
    }
}
