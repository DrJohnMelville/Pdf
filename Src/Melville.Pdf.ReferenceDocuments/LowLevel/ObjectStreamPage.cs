using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ReferenceDocuments.LowLevel;

public class ObjectStreamPage: CreatePdfParser
{
    public ObjectStreamPage() : base("Document using an object stream.")
    {
    }

    public override async ValueTask WritePdfAsync(Stream target) =>
        await (await FiltersAsync()).WriteToWithXrefStreamAsync(target);

    public static async ValueTask<PdfLowLevelDocument> FiltersAsync()
    {
        var creator = new PdfDocumentCreator();
        using (creator.LowLevelCreator.ObjectStreamContext(
                         new DictionaryBuilder()))
        {
            var page = creator.Pages.CreatePageInObjectStream();
            var fontName = page.AddStandardFont("F1", BuiltInFontName.Helvetica, FontEncodingName.WinAnsiEncoding);
            await page.AddToContentStreamAsync(i=>
            {
                using var block = i.StartTextBlock();
                i.SetFontAsync(fontName, 24);
                block.MovePositionBy(100,700);
                block.ShowStringAsync("Uses Object String");
            });
        }
        return creator.CreateDocument();
    }
}