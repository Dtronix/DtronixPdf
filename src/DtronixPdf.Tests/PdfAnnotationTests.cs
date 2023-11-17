using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DtronixPdf.Tests
{
    public class PdfAnnotationTests
    {
        [Test]
        public void LoadsDocument()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            Assert.AreEqual(1, document.Pages);
        }

        [Test]
        public void LoadsMemoryDocument()
        {
            using var stream = File.OpenRead("TestPdf.pdf");
            using var document = PdfDocument.Load(stream, null);
            Assert.AreEqual(1, document.Pages);
        }

        [Test]
        public void SavesDocument()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var sw = new MemoryStream();
            document.Save(sw);

            Assert.Greater(sw.Length, 10000);
        }


    }
}
