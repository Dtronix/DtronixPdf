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
        public async Task RendererCreatesImageSize()
        {
            await using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
            var renderPage = await page.RenderAsync(
                1,
                (uint)Color.White.ToArgb(),
                new Boundary(0, 0, page.Width, page.Height));

            var image = renderPage.GetImage();

            Assert.AreEqual(page.Width, image.Width);
            Assert.AreEqual(page.Height, image.Height);
        }

        [Test]
        public async Task RendererSavesImage()
        {
            await using var document = await PdfDocument.LoadAsync("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
            var renderPage = await page.RenderAsync(
                1,
                (uint)Color.White.ToArgb(),
                new Boundary(0, 0, page.Width, page.Height));

            await using var writer = File.OpenWrite("test.png");
            await renderPage.GetImage().SaveAsync(writer, new PngEncoder());
        }

        /*
        public async Task PixelTest_1()
        {
            await using var document = await PdfDocument.Load("TestPdf.pdf", null);
            await using var page = await document.GetPageAsync(0);
            await using var expectedImageStream = File.OpenRead("PdfPageRendererTests/pixel_test_1.png");
            using var expectedImage = MediaTypeNames.Image.Load<Bgra32>(expectedImageStream, new PngDecoder());

            var renderPage = await page.Render(
                1,
                (uint)Color.White.ToArgb(), 
                new Boundary(522, 477, 3, 3));

            var renderPage2 = await page.Render(
                1,
                (uint)Color.White.ToArgb(), 
                new Boundary(522, 477, 30, 30));

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
        }*/
    }
}