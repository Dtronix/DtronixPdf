using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;
using DtronixPdf;
using DtronixPdf.ImageSharp;
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

            RenderViewportScaling();
            OpenAndCloseBenchmark();


            Console.ReadLine();
        }

        static async Task RenderViewportScaling()
        {
            Console.WriteLine($"RenderViewport Benchmark {TestPdf}");
            var document = PdfDocument.Load(TestPdf, null);

            sw.Restart();
            var iterations = 25;

            for (int i = 1; i < iterations; i++)
            {
                using var page = document.GetPage(0);
                
                float scale = i * 0.25f;
                Point center = new Point(0, 0);
                Size size = new Size(1920, 1080);

                var viewport = Vector128.Create(
                    0f, 
                    0f,
                    1920f,
                    1080f);

                using var result = page.Render(new PdfPageRenderConfig()
                {
                    Viewport = viewport,
                    Scale = scale,
                    BackgroundColor = uint.MaxValue
                });
                result.GetImage().SaveAsPng($"output/{TestPdf}-{i}.png");
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
                using var document = PdfDocument.Load(TestPdf, null);
                using var page = document.GetPage(0);

                Console.WriteLine($"{sw.ElapsedMilliseconds:##,###} Milliseconds");
                sw.Restart();
            }

            sw.Stop();

            Console.WriteLine($"Open and Close {TestPdf} Complete");
        }

        static async Task ImportTests()
        {
            var drawing = PdfDocument.Load("drawing.pdf", null);
            var testDocument = PdfDocument.Load("testdoc1.pdf", null);

            var newDocument = PdfDocument.Create();

            newDocument.ImportPages(drawing, "1", 0);
            newDocument.ImportPages(testDocument, null, 1);
            newDocument.ImportPages(drawing, "1", 2);

            var page = newDocument.GetPage(1);

            newDocument.Save("output/importtests.pdf");

        }
    }
}
