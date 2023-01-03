using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class EncodingWithDiferences : FontDefinitionTest
{

    public EncodingWithDiferences() : base("Uses an encoding dictionary to make a different font array")
    {
        TextToRender = "ABC DE";
    }

    protected override PdfObject CreateFont(IPdfObjectRegistry arg)
    {
        var enc = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Differences, new PdfArray(
                65,
                NameDirectory.Get("AE"),
                NameDirectory.Get("Adieresis"),
                NameDirectory.Get("ff")
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