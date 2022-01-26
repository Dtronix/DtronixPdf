using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtronixPdf.Dispatcher;
using PDFiumCore;

namespace DtronixPdf
{

    public class PDFiumManager
    {
        internal readonly ThreadDispatcher Dispatcher;

        private static int Instances = 0;

        private static PDFiumManager _managerDefaultInstance;

        private static bool FPDF_InitLibrary;

        public static PDFiumManager Default => _managerDefaultInstance ??= new PDFiumManager();

        public PDFiumManager()
        {
            Dispatcher = new ThreadDispatcher();
            Dispatcher.Start();

            if (!FPDF_InitLibrary)
                Initialize();
        }

        private async Task Initialize()
        {
            FPDF_InitLibrary = true;
            // Initialize the library.
            await Dispatcher.QueueForCompletion(fpdfview.FPDF_InitLibrary);
        }
    }
}
