using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel;

public class MinimalPdfParser: CreatePdfParser
{
    public MinimalPdfParser() : base("-min", "Minimal pdf with a single blank page.")
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