using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DtronixCommon;
using DtronixPdf;
using DtronixPdf.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DtronixPdfBenchmark
{
    class Program
    {
        static Stopwatch sw = new Stopwatch();
        private const string TestPdf = "TestPdf.pdf";
        static async Task Main(string[] args)
        {
            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");

            await RenderViewportScaling();
            await OpenAndCloseBenchmark();


            Console.ReadLine();
        }

        static async Task RenderViewportScaling()
        {
            Console.WriteLine($"RenderViewport Benchmark {TestPdf}");
            var document = await PdfDocument.LoadAsync(TestPdf, null);

            sw.Restart();
            var iterations = 25;

            for (int i = 1; i < iterations; i++)
            {
                using var page = await document.GetPageAsync(0);
                
                float scale = i * 0.25f;
                Point center = new Point(0, 0);
                Size size = new Size(1920, 1080);

                var viewport = new Boundary(
                    (int)((page.Width / 2 - size.Width / 2 + center.X) * scale + size.Width / 2 * (scale - 1)),
                    (int)((page.Height / 2 - size.Height / 2 - center.Y) * scale + size.Height / 2 * (scale - 1)),
                    size.Width,
                    size.Height);


                await using var result = await page.RenderAsync(
                    scale,
                    Color.White.ToPixel<Argb32>().Argb,
                    viewport);
                await result.GetImage().SaveAsPngAsync($"output/{TestPdf}-{i}.png");
                Console.WriteLine($"{sw.ElapsedMilliseconds:##,###} Milliseconds");
                sw.Restart();
            }

            sw.Stop();
            document.Dispose();

            Console.WriteLine($"Rendering {TestPdf} Complete");
        }

        static async Task OpenAndCloseBenchmark()
        {
            Console.WriteLine($"Open and Close {TestPdf}");
            sw.Restart();
            var iterations = 100;

            for (int i = 1; i < iterations; i++)
            {
                using var document = await PdfDocument.LoadAsync(TestPdf, null);
                using var page = await document.GetPageAsync(0);

                Console.WriteLine($"{sw.ElapsedMilliseconds:##,###} Milliseconds");
                sw.Restart();
            }

            sw.Stop();

            Console.WriteLine($"Open and Close {TestPdf} Complete");
        }

        static async Task ImportTests()
        {
            var drawing = await PdfDocument.LoadAsync("drawing.pdf", null);
            var testDocument = await PdfDocument.LoadAsync("testdoc1.pdf", null);

            var newDocument = await PdfDocument.CreateAsync();

            await newDocument.ImportPagesAsync(drawing, "1", 0);
            await newDocument.ImportPagesAsync(testDocument, null, 1);
            await newDocument.ImportPagesAsync(drawing, "1", 2);

            var page = await newDocument.GetPageAsync(1);

            await newDocument.SaveAsync("output/importtests.pdf");

        }
    }
}
