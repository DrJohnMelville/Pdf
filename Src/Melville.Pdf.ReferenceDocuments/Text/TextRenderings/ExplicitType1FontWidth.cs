using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.TextRenderings;

public class ExplicitType1FontWidth : FontDefinitionTest
{
    public ExplicitType1FontWidth() :
        base("Uses a built in font but specifies font widths")
    {
        TextToRender = "ABCABC";
    }

    protected override PdfObject CreateFont(IPdfObjectRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, BuiltInFontName.Helvetica)
            .WithItem(KnownNames.FirstChar, 'A')
            .WithItem(KnownNames.LastChar, 'C')
            .WithItem(KnownNames.Widths, new PdfArray(
                250,
                1500,
                1000
            ))
            .AsDictionary();
}