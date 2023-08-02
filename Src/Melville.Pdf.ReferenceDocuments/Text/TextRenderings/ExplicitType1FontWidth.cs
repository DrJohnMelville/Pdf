
namespace Melville.Pdf.ReferenceDocuments.Text.TextRenderings;

public class ExplicitType1FontWidth : FontDefinitionTest
{
    public ExplicitType1FontWidth() :
        base("Uses a built in font but specifies font widths")
    {
        TextToRender = "ABCABC";
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, (PdfDirectObject)BuiltInFontName.Helvetica)
            .WithItem(KnownNames.FirstCharTName, 'A')
            .WithItem(KnownNames.LastCharTName, 'C')
            .WithItem(KnownNames.WidthsTName, new PdfArray(
                250,
                1500,
                1000
            ))
            .AsDictionary();
}