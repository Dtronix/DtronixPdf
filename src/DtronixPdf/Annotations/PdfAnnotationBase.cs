using System;
using System.Reflection.Metadata;
using System.Threading;
using DtronixCommon;
using PDFiumCore;

namespace DtronixPdf.Annotations;

public abstract class PdfAnnotationBase : IDisposable
{
    protected readonly PdfPage Page;
    protected readonly AnnotationType Type;
    protected readonly FpdfAnnotationT Annotation;

    private bool _isDisposed = false;


    protected PdfAnnotationBase(PdfPage page, AnnotationType type, FpdfAnnotationT annotation)
    {
        Page = page;
        Type = type;
        Annotation = annotation;
    }

    /*
    public static PdfAnnotation Create(PdfPage page, AnnotationType type)
    {
        var annotation = PdfActionSync.Default.SyncExec(() => fpdf_annot.FPDFPageCreateAnnot(page.PageInstance, (int)type));
        
        if (annotation == null)
            throw new Exception("Unable to create annotation.");
        
        return new PdfAnnotation(page, type, annotation); 
    }*/

    public virtual void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        OnDispose();

        PdfActionSync.Default.SyncExec(() => fpdf_annot.FPDFPageCloseAnnot(Annotation));
    }

    protected abstract void OnDispose();
}
