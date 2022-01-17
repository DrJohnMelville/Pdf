namespace Melville.Pdf.ReferenceDocuments.Text;

public class EncodingWithDiferences : FontDefinitionTest
{
    
    public EncodingWithDiferences() : base("Uses an encoding dictionary to make a different font array")
    {
        TextToRender = "ABC DE";
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var enc = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Differences, new PdfArray(
                new PdfInteger(65),
                NameDirectory.Get("AE"),
                NameDirectory.Get("Adieresis")
                ))
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, BuiltInFontName.Courier)
            .WithItem(KnownNames.Encoding, enc)
            .AsDictionary();
    }
}