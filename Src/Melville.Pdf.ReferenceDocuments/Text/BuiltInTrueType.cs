namespace Melville.Pdf.ReferenceDocuments.Text;

public class BuiltInTrueType : FontDefinitionTest
{
    public BuiltInTrueType() : base("Uses a TrueType font from the operating system")
    {
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("CooperBlack"))
            .AsDictionary();
}

public class ExplicitType1FontWidth: FontDefinitionTest
{
    public ExplicitType1FontWidth() : 
        base("Uses a built in font but specifies font widths")
    {
        TextToRender = "ABCABC";
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, BuiltInFontName.Helvetica)
            .WithItem(KnownNames.FirstChar, 'A')
            .WithItem(KnownNames.LastChar, 'C')
            .WithItem(KnownNames.Widths, new PdfArray(
                    new PdfInteger(250),
                    new PdfInteger(1500),
                    new PdfInteger(1000)
                ))
            .AsDictionary();
}