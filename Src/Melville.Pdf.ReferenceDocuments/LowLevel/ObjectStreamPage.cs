using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ReferenceDocuments.LowLevel;

public class ObjectStreamPage: CreatePdfParser
{
    public ObjectStreamPage() : base("-ObjectStream", "Document using an object stream.")
    {
    }

    public override async ValueTask WritePdfAsync(Stream target) =>
        await (await FiltersAsync()).WriteToWithXrefStreamAsync(target);

    public static async ValueTask<PdfLowLevelDocument> FiltersAsync()
    {
        var creator = new PdfDocumentCreator();
        await using (creator.LowLevelCreator.ObjectStreamContext(
                         new DictionaryBuilder()))
        {
            var page = creator.Pages.CreatePageInObjectStream();
            page.AddStandardFont("F1", BuiltInFontName.Helvetica, FontEncodingName.WinAnsiEncoding);
            page.AddToContentStream(new DictionaryBuilder().AsStream(
                "BT\n/F1 24 Tf\n100 700 Td\n(Uses Object Stream) Tj\nET\n"));
        }
        return creator.CreateDocument();
    }
}