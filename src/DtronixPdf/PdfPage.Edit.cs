using System;
using System.Threading.Tasks;
using PDFiumCore;

namespace DtronixPdf
{
    public partial class PdfPage : IAsyncDisposable
    {
        
        /// <summary>
        /// Rotates the page
        /// </summary>
        /// <param name="rotation">
        /// <para>0 - No rotation.</para>
        /// <para>1 - Rotated 90 degrees clockwise.</para>
        /// <para>2 - Rotated 180 degrees clockwise.</para>
        /// <para>3 - Rotated 270 degrees clockwise.</para>
        /// </param>
        /// <returns></returns>
        public Task SetRotation(int rotation)
        {
            return _dispatcher.Queue(() => fpdf_edit.FPDFPageSetRotation(PageInstance, rotation));
        }

        /// <summary>
        /// Rotates the page
        /// </summary>
        /// <returns>
        /// <para>0 - No rotation.</para>
        /// <para>1 - Rotated 90 degrees clockwise.</para>
        /// <para>2 - Rotated 180 degrees clockwise.</para>
        /// <para>3 - Rotated 270 degrees clockwise.</para>
        /// </returns>
        public Task<int> GetRotation()
        {
            return _dispatcher.QueueResult(_ => fpdf_edit.FPDFPageGetRotation(PageInstance));
        }

        /// <summary>
        /// Rotates the page
        /// </summary>
        /// <param name="rotation">
        /// <para>0 - No rotation.</para>
        /// <para>1 - Rotated 90 degrees clockwise.</para>
        /// <para>2 - Rotated 180 degrees clockwise.</para>
        /// <para>3 - Rotated 270 degrees clockwise.</para>
        /// </param>
        /// <returns></returns>
        public Task Delete()
        {
            return _dispatcher.Queue(() => fpdf_edit.FPDFPageDelete(_documentInstance, InitialIndex));
        }
    }
}