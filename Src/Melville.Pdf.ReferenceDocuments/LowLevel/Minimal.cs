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
        await MinimalPdf(1, 7).CreateDocument().WriteToAsync(target);

    public static ILowLevelDocumentCreator MinimalPdf(int major, int minor)
    {
        var builder = new PdfDocumentCreator();
        builder.LowLevelCreator.SetVersion((byte)major, (byte)minor);
        builder.Pages.CreatePage();
        builder.CreateDocument();
        return builder.LowLevelCreator;
    }
}