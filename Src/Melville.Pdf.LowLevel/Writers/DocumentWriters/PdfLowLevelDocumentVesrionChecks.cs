using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

internal static class PdfLowLevelDocumentVesrionChecks
{
    public static void VerifyCanSupportObjectStreams(this PdfLowLevelDocument document)
    {
        if (document.MajorVersion < 2 && document.MinorVersion < 5)
            throw new PdfParseException("Object streams unavailable before pdf version 1.5");
    }
}