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
        public void SetRotation(int rotation)
        {
            PdfiumManager.Default.Synchronizer.SyncExec(() => fpdf_edit.FPDFPageSetRotation(PageInstance, rotation));
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
            return PdfiumManager.Default.Synchronizer.SyncExec(() => fpdf_edit.FPDFPageGetRotation(PageInstance));
        }


        /// <summary>
        /// Deletes the page.
        /// </summary>
        public void Delete()
        {
            PdfiumManager.Default.Synchronizer.SyncExec(() => fpdf_edit.FPDFPageDelete(Document.Instance, InitialIndex));
        }
    }
}
