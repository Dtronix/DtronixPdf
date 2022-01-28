using System.Threading.Tasks;
using NUnit.Framework;

namespace DtronixPdf.Tests
{
    public class Tests
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
        public async Task Test1()
        {
            await using var document = await PdfDocument.Load("TestPdf.pdf", null);
            Assert.AreEqual(1, document.Pages);
        }


    }
}