using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

public readonly struct PdfPageParent : IHasPageAttributes
{
    public PdfDictionary LowLevel { get; }

    public PdfPageParent(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }
}