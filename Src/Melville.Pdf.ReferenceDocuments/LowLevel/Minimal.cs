using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ReferenceDocuments.LowLevel;

public class MinimalPdfParser: CreatePdfParser
{
    public MinimalPdfParser() : base("Minimal pdf with a single blank page.")
    {
    }

    public override async ValueTask WritePdfAsync(Stream target) =>
        await MinimalPdf().WriteToAsync(target);

    public static PdfLowLevelDocument MinimalPdf(byte major = 1, byte minor = 7)
    {
        var builder = new PdfDocumentCreator();
        builder.Pages.CreatePage();
        return builder.CreateDocument(major, minor);
    }
}