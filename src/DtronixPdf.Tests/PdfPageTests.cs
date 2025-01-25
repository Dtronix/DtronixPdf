using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace DtronixPdf.Tests
{
    public class PdfPageTests
    {
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

        [Test]
        public void GetTextReturnsText()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);

            string actual = page.GetText(0, 150, page.Width, 500);
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                actual);
        }
    }
}
