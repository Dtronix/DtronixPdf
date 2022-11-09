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
        public void LoadsPage()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
        }

        [Test]
        public void PageSizeIsReturned()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
            Assert.AreEqual(new SizeF(792, 612), new SizeF(page.Width, page.Height));
        }

        [Test]
        public void InitalIndexIsReturned()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
            Assert.AreEqual(0, page.InitialIndex);
        }

        [Test]
        public void GetRotationReturnsCorrectValue()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
            Assert.AreEqual(3, page.GetRotation());
        }

        [Test]
        public void SetRotationSetsValue()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
            page.SetRotation(1);
            Assert.AreEqual(1, page.GetRotation());
        }
    }
}
