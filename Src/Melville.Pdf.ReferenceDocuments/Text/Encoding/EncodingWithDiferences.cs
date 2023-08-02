
namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class EncodingWithDiferences : FontDefinitionTest
{

    public EncodingWithDiferences() : base("Uses an encoding dictionary to make a different font array")
    {
        TextToRender = "ABC DE";
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var enc = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Differences, new PdfArray(
                65,
                PdfDirectObject.CreateName("AE"),
                PdfDirectObject.CreateName("Adieresis"),
                PdfDirectObject.CreateName("ff")
            ))
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, (PdfDirectObject)BuiltInFontName.Courier)
            .WithItem(KnownNames.Encoding, enc)
            .AsDictionary();
    }
}