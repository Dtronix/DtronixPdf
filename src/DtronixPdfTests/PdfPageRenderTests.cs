using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DtronixPdf.Tests
{
    public class PdfPageRenderTests
    {
        [Test]
        public async Task RendererCreatesImageSize()
        {
            await using var document = await PdfDocument.Load("TestPdf.pdf", null);
            await using var page = await document.GetPage(0);
            var renderPage = await page.Render(
                1,
                Color.White,
                new RectangleF(0, 0, page.Size.Width, page.Size.Height));

            Assert.AreEqual(page.Size.Width, renderPage.Image.Width);
            Assert.AreEqual(page.Size.Height, renderPage.Image.Height);
        }

        [Test]
        public async Task RendererSavesImage()
        {
            await using var document = await PdfDocument.Load("TestPdf.pdf", null);
            await using var page = await document.GetPage(0);
            var renderPage = await page.Render(
                1,
                Color.White,
                new RectangleF(0, 0, page.Size.Width, page.Size.Height));

            await using var writer = File.OpenWrite("test.png");
            await renderPage.Image.SaveAsync(writer, PngFormat.Instance);
        }

        
        public async Task PixelTest_1()
        {
            await using var document = await PdfDocument.Load("TestPdf.pdf", null);
            await using var page = await document.GetPage(0);
            await using var expectedImageStream = File.OpenRead("PdfPageRendererTests/pixel_test_1.png");
            using var expectedImage = Image.Load<Bgra32>(expectedImageStream, new PngDecoder());
            var renderPage = await page.Render(1,
                Color.White, new RectangleF(522, 477, 3, 3));

            var renderPage2 = await page.Render(1,
                Color.White, new RectangleF(522, 477, 30, 30));

            await renderPage2.Image.SaveAsPngAsync("png1Crop.png");

            var renderPage3 = await page.Render(1, Color.White);

            await renderPage3.Image.SaveAsPngAsync("png1Full.png");











            var pageBitmap = await page.Render(1, Color.White);
            
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Assert.AreEqual(expectedImage[x, y], pageBitmap.Image[x, y]);
                }
            }
        }
    }
}