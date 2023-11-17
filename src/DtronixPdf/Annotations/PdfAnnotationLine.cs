using System;
using System.Reflection.Metadata;
using System.Threading;
using DtronixCommon;
using PDFiumCore;

namespace DtronixPdf.Annotations;

public sealed class PdfAnnotationLine : PdfAnnotationBase
{
    private PdfAnnotationLine(PdfPage page, FpdfAnnotationT annotation)
        :base(page, AnnotationType.Line, annotation)
    {
        var obj = fpdf_annot.FPDFAnnotGetObject(annotation, 0);
    }


    public static PdfAnnotationLine Create(PdfPage page)
    {
        var annotation = PdfActionSync.Default.SyncExec(() => 
            fpdf_annot.FPDFPageCreateAnnot(page.PageInstance, (int)AnnotationType.Line));

        if (annotation == null)
            throw new Exception("Unable to create annotation.");
        
        return new PdfAnnotationLine(page, annotation); 
    }

    protected override void OnDispose()
    {
        
    }
}
