using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DtronixPdf;
using DtronixPdf.Dispatcher;
using PDFiumCore;
using SixLabors.ImageSharp;

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
            var document = await PdfDocument.Load(TestPdf, null);

            sw.Restart();
            var iterations = 25;

            for (int i = 1; i < iterations; i++)
            {
                await using var page = await document.GetPage(0);
                
                float scale = i * 0.25f;
                Point center = new Point(0, 0);
                Size size = new Size(1920, 1080);

                var viewport = new RectangleF(
                    (int)((page.Size.Width / 2 - size.Width / 2 + center.X) * scale + size.Width / 2 * (scale - 1)),
                    (int)((page.Size.Height / 2 - size.Height / 2 - center.Y) * scale + size.Height / 2 * (scale - 1)),
                    size.Width,
                    size.Height);


                await using var result = await page.Render(
                    scale,
                    Color.White,
                    viewport);
                await result.Image.SaveAsPngAsync($"output/{TestPdf}-{i}.png");
                Console.WriteLine($"{sw.ElapsedMilliseconds:##,###} Milliseconds");
                sw.Restart();
            }

            sw.Stop();
            await document.DisposeAsync();

            Console.WriteLine($"Rendering {TestPdf} Complete");
        }

        static async Task OpenAndCloseBenchmark()
        {
            Console.WriteLine($"Open and Close {TestPdf}");
            sw.Restart();
            var iterations = 100;

            for (int i = 1; i < iterations; i++)
            {
                await using var document = await PdfDocument.Load(TestPdf, null);
                await using var page = await document.GetPage(0);

                Console.WriteLine($"{sw.ElapsedMilliseconds:##,###} Milliseconds");
                sw.Restart();
            }

            sw.Stop();

            Console.WriteLine($"Open and Close {TestPdf} Complete");
        }

        static async Task ImportTests()
        {
            var drawing = await PdfDocument.Load("drawing.pdf", null);
            var testDocument = await PdfDocument.Load("testdoc1.pdf", null);

            var newDocument = await PdfDocument.Create();

            await newDocument.ImportPages(drawing, "1", 0);
            await newDocument.ImportPages(testDocument, null, 1);
            await newDocument.ImportPages(drawing, "1", 2);

            var page = await newDocument.GetPage(1);

            await newDocument.Save("output/importtests.pdf");

        }
    }
}