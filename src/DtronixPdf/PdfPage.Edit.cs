using System;
using System.Threading;
using System.Threading.Tasks;
using DtronixPdf.Actions;
using DtronixPdf.Dispatcher;
using PDFiumCore;
using SixLabors.ImageSharp;

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
            return _dispatcher.QueueForCompletion(() => fpdf_edit.FPDFPageSetRotation(_pageInstance, rotation));
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
            return _dispatcher.QueueWithResult(() => fpdf_edit.FPDFPageGetRotation(_pageInstance));
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
            return _dispatcher.QueueForCompletion(() => fpdf_edit.FPDFPageDelete(_documentInstance, InitialIndex));
        }
    }
}