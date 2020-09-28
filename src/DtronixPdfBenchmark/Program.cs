using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using DtronixPdf;
using PDFiumCore;

namespace DtronixPdfBenchmark
{
    class Program
    {
        static Stopwatch sw = new Stopwatch();
        static async Task Main(string[] args)
        {
            try
            {
                var drawing = await PdfDocument.Load("drawing.pdf", null);
                var testDocument = await PdfDocument.Load("testdoc1.pdf", null);

                var newDocument = await PdfDocument.Create();

                await newDocument.ImportPages(drawing, "1", 0);
                await newDocument.ImportPages(testDocument, null, 1);
                await newDocument.ImportPages(drawing, "1", 2);

                var page = await newDocument.GetPage(1);
                await newDocument.DeletePage(1);

                page = await newDocument.GetPage(1);

                var render = await page.Render(RenderFlags.RenderAnnotations);

                render.Bitmap.Save("g1.png");

                await newDocument.Save("output.pdf");

                sw.Start();
                var iterations = 1;
                //await using var page = await drawing.GetPage(0);

                for (int i = 0; i < iterations; i++)
                {
                    await using var result = await page.Render(RenderFlags.RenderAnnotations, 1, (500, 1500, 500, 1000), Color.Aqua);
                    //await using var result = await page.Render(RenderFlags.RenderAnnotations, 1,(0,0,0,0), Color.White);
                    result.Bitmap.Save("test.png");
                }

                sw.Stop();
                await drawing.DisposeAsync();

                Console.WriteLine($"Rendererd pdf {iterations} times in {sw.ElapsedMilliseconds:##,###}ms; {sw.ElapsedMilliseconds/ iterations}ms per.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Console.ReadLine();
        }
    }
}
