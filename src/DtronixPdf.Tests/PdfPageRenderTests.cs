using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using DtronixCommon;
using DtronixPdf.ImageSharp;
using NUnit.Framework;
using PDFiumCore;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DtronixPdf.Tests
{
    public class PdfPageRenderTests
    {
        [Test]
        public void RendererCreatesImageSize()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
            var renderPage = page.Render(
                1,
                (uint)Color.White.ToArgb(),
                new Boundary(0, 0, page.Width, page.Height));

            var image = renderPage.GetImage();

            Assert.AreEqual(page.Width, image.Width);
            Assert.AreEqual(page.Height, image.Height);
        }

        [Test]
        public void RendererSavesImage()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
            var renderPage = page.Render(
                1,
                (uint)Color.White.ToArgb(),
                new Boundary(0, 0, page.Width, page.Height));

            using var writer = File.OpenWrite("test.png");
            renderPage.GetImage().Save(writer, new PngEncoder());
        }

        /*
        public void PixelTest_1()
        {
            using var document = PdfDocument.Load("TestPdf.pdf", null);
            using var page = document.GetPage(0);
            using var expectedImageStream = File.OpenRead("PdfPageRendererTests/pixel_test_1.png");
            using var expectedImage = MediaTypeNames.Image.Load<Bgra32>(expectedImageStream, new PngDecoder());

            var renderPage = page.Render(
                1,
                (uint)Color.White.ToArgb(), 
                new Boundary(522, 477, 3, 3));

            var renderPage2 = page.Render(
                1,
                (uint)Color.White.ToArgb(), 
                new Boundary(522, 477, 30, 30));

            renderPage2.Image.SaveAsPng("png1Crop.png");

            var renderPage3 = page.Render(1, Color.White);

            renderPage3.Image.SaveAsPng("png1Full.png");











            var pageBitmap = page.Render(1, Color.White);
            
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Assert.AreEqual(expectedImage[x, y], pageBitmap.Image[x, y]);
                }
            }
        }*/
    }
}
