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


        [Test]
        public async Task LoadsPage()
        {
            await using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
        }

        [Test]
        public async Task PageSizeIsReturned()
        {
            await using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
            Assert.AreEqual(new SizeF(792, 612), new SizeF(page.Width, page.Height));
        }

        [Test]
        public async Task InitalIndexIsReturned()
        {
            await using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
            Assert.AreEqual(0, page.InitialIndex);
        }

        [Test]
        public async Task GetRotationReturnsCorrectValue()
        {
            await using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
            Assert.AreEqual(3, await page.GetRotation());
        }

        [Test]
        public async Task SetRotationSetsValue()
        {
            await using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
            await page.SetRotation(1);
            Assert.AreEqual(1, await page.GetRotation());
        }
    }
}