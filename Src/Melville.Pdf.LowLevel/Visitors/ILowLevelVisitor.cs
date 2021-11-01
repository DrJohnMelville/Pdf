using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Visitors;

public interface ILowLevelVisitor<out T>
{
    T Visit(PdfArray item);
    T Visit(PdfBoolean item);
    T Visit(PdfDictionary item);
    T Visit(PdfTokenValues item);
    T Visit(PdfIndirectObject item);
    T Visit(PdfIndirectReference item);
    T Visit(PdfName item);
    T Visit(PdfInteger item);
    T Visit(PdfDouble item);
    T Visit(PdfString item);
    T Visit(PdfStream item);
    T Visit(PdfFreeListObject item);
}