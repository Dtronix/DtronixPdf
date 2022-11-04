using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DtronixPdf.Tests
{
    public class PdfDocumentTests
    {
        //[Test]
        public async Task LoadsDocument()
        {
            var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            Assert.AreEqual(1, document.Pages);
        }

        [Test]
        public void LoadsMemoryDocument()
        {
            using var stream = File.OpenRead("TestPdf.pdf");
            using var document = PdfDocument.Load(stream, null);
            Assert.AreEqual(1, document.Pages);
        }

        //[Test]
        public async Task SavesDocument()
        {
            var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var sw = new MemoryStream();
            await document.SaveAsync(sw);

            Assert.Greater(sw.Length, 10000);
        }


    }
}
