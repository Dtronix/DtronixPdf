using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace DtronixPdf.Tests
{
    public class PdfPageTests
    {

        [SetUp]
        public void Setup()
        {
            //PDFiumCoreManager.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            //PDFiumCoreManager.Destroy();
        }


        //[Test]
        public async Task LoadsPage()
        {
            using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            using var page = await document.GetPageAsync(0);
        }

        //[Test]
        public async Task PageSizeIsReturned()
        {
            using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            using var page = await document.GetPageAsync(0);
            Assert.AreEqual(new SizeF(792, 612), new SizeF(page.Width, page.Height));
        }

        //[Test]
        public async Task InitalIndexIsReturned()
        {
            using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            using var page = await document.GetPageAsync(0);
            Assert.AreEqual(0, page.InitialIndex);
        }

        //[Test]
        public async Task GetRotationReturnsCorrectValue()
        {
            using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            using var page = await document.GetPageAsync(0);
            Assert.AreEqual(3, await page.GetRotationAsync());
        }

        //[Test]
        public async Task SetRotationSetsValue()
        {
            using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            using var page = await document.GetPageAsync(0);
            await page.SetRotationAsync(1);
            Assert.AreEqual(1, await page.GetRotationAsync());
        }
    }
}
