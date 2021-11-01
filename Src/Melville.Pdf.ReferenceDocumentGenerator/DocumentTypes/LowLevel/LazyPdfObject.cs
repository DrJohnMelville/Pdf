using System;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel;

public class LazyPdfObject
{
    private PdfIndirectReference? reference;
    private Func<PdfObject> definition;
    private ILowLevelDocumentCreator creator;

    public LazyPdfObject(ILowLevelDocumentCreator creator, Func<PdfObject> definition)
    {
        this.definition = definition;
        this.creator = creator;
    }

    public PdfIndirectReference Value
    {
        get
        {
            reference ??= creator.AsIndirectReference(definition());
            return reference;
        }
    }

    public bool HasValue => reference != null;
}