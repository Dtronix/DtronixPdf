using System;
using System.Threading.Tasks;
using Mono.TextTemplating;
using PDFiumCore;

namespace DtronixPdf
{
    public partial class PdfPage
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
        public Task SetRotationAsync(int rotation)
        {
            return Document.Dispatcher.Queue(() => fpdf_edit.FPDFPageSetRotation(PageInstance, rotation));
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
        public void SetRotation(int rotation)
        {
            Document.Dispatcher.SyncExec(() => fpdf_edit.FPDFPageSetRotation(PageInstance, rotation));
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
        public Task<int> GetRotationAsync()
        {
            return Document.Dispatcher.QueueResult(_ => fpdf_edit.FPDFPageGetRotation(PageInstance));
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
        public int GetRotation()
        {
            return Document.Dispatcher.SyncExec(() => fpdf_edit.FPDFPageGetRotation(PageInstance));
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
        public Task DeleteAsync()
        {
            return Document.Dispatcher.Queue(() => fpdf_edit.FPDFPageDelete(Document.Instance, InitialIndex));
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
        public void Delete()
        {
            Document.Dispatcher.SyncExec(() => fpdf_edit.FPDFPageDelete(Document.Instance, InitialIndex));
        }
    }
}
