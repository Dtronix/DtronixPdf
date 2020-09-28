using System;
using System.IO;
using PDFiumCore;

namespace DtronixPdf
{
    class PdfFileWriteCopyStream : FPDF_FILEWRITE_
    {
        public Stream WriteStream { get; }

        public PdfFileWriteCopyStream(Stream writeStream)
        {
            WriteStream = writeStream;
            WriteBlock = CopyToStream;
        }

        private unsafe int CopyToStream(IntPtr pthis, IntPtr pdata, uint size)
        {
            var reader = new UnmanagedMemoryStream((byte*) pdata, size);
            reader.CopyTo(WriteStream);
            return 1;
        }
    }
}