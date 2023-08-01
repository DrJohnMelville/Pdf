using Melville.Pdf.LowLevel.Model.Primitives;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.ReferenceDocuments.Text.TextRenderings;

public class ExplicitType1FontWidth : FontDefinitionTest
{
    public ExplicitType1FontWidth() :
        base("Uses a built in font but specifies font widths")
    {
        TextToRender = "ABCABC";
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, (PdfDirectValue)BuiltInFontName.Helvetica)
            .WithItem(KnownNames.FirstCharTName, 'A')
            .WithItem(KnownNames.LastCharTName, 'C')
            .WithItem(KnownNames.WidthsTName, new PdfValueArray(
                250,
                1500,
                1000
            ))
            .AsDictionary();
}