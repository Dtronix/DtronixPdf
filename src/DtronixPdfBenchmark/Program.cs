using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using DtronixPdf;
using DtronixPdf.Dispatcher;
using PDFiumCore;

namespace DtronixPdfBenchmark
{
    class Program
    {
        static Stopwatch sw = new Stopwatch();

        static async Task Main(string[] args)
        {
            await RenderViewport();
            Console.ReadLine();
        }

        static async Task RenderViewport()
        {
            var drawing = await PdfDocument.Load("TestPdf.pdf", null);
            await using var page = await drawing.GetPage(0);


            sw.Start();
            var iterations = 2;
            //await using var page = await drawing.GetPage(0);
            var viewport = new Rectangle(500, 50, 500, 500);

            for (int i = 1; i < iterations; i++)
            {
                /*
                 * new BitmapClip(
                        viewport.Left * scale + viewport.Width / 2 * (scale - 1), 
                        viewport.Top * scale + viewport.Height / 2 * (scale - 1), 
                        (page.Size.Width - viewport.Width - viewport.Left) * scale + viewport.Width / 2 * (scale - 1), 
                        ((page.Size.Height - viewport.Height - viewport.Top) * scale) + viewport.Height / 2 * (scale - 1)), 
                 */
                float scale = i;
                Point center = new Point(0, 0);
                Size size = new Size(1920, 1080);
                
                await using var result = await page.Render(RenderFlags.RenderAnnotations, scale,
                    new Rectangle((int) ((page.Size.Width / 2 - size.Width / 2 + center.X) * scale + size.Width / 2 * (scale - 1)),
                        (int) ((page.Size.Height / 2 - size.Height / 2 - center.Y) * scale + size.Height / 2 * (scale - 1)),
                        size.Width,
                        size.Height),
                    false, Color.White, default, DispatcherPriority.Normal);
                Console.WriteLine($"{sw.ElapsedMilliseconds:##,###}");
                result.ToBitmap().Save($"test{i}.png");
                sw.Restart();
            }

            sw.Stop();
            await drawing.DisposeAsync();
        }

        static async Task RenderTests()
        {
            var drawing = await PdfDocument.Load("drawing.pdf", null);
            /*var testDocument = await PdfDocument.Load("testdoc1.pdf", null);

            var newDocument = await PdfDocument.Create();

            await newDocument.ImportPages(drawing, "1", 0);
            await newDocument.ImportPages(testDocument, null, 1);
            await newDocument.ImportPages(drawing, "1", 2);

            var page = await newDocument.GetPage(1);
            await newDocument.DeletePage(1);

            page = await newDocument.GetPage(1);
            var page = await drawing.GetPage(0);

            var mem = Environment.WorkingSet;

            sw.Restart();
            var render = await page.Render(RenderFlags.RenderAnnotations);
            Console.WriteLine($"RGB Memory: {Environment.WorkingSet - mem:##,###}; {sw.ElapsedMilliseconds:##,###}");

            for (int i = 0; i < 10; i++)
            {
                sw.Restart();
                mem = Environment.WorkingSet;
                render = await page.Render(RenderFlags.RenderAnnotations);
                Console.WriteLine($"RGB Memory: {Environment.WorkingSet - mem:##,###}; {sw.ElapsedMilliseconds:##,###}");
            }*/
        }

        static async Task ImportTests()
        {
            var drawing = await PdfDocument.Load("drawing.pdf", null);
            /*var testDocument = await PdfDocument.Load("testdoc1.pdf", null);

            var newDocument = await PdfDocument.Create();

            await newDocument.ImportPages(drawing, "1", 0);
            await newDocument.ImportPages(testDocument, null, 1);
            await newDocument.ImportPages(drawing, "1", 2);

            var page = await newDocument.GetPage(1);
            await newDocument.DeletePage(1);

            page = await newDocument.GetPage(1);*/
        }
    }
}