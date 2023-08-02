
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
            .WithItem(KnownNames.DifferencesTName, new PdfArray(
                65,
                PdfDirectObject.CreateName("AE"),
                PdfDirectObject.CreateName("Adieresis"),
                PdfDirectObject.CreateName("ff")
            ))
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, (PdfDirectObject)BuiltInFontName.Courier)
            .WithItem(KnownNames.EncodingTName, enc)
            .AsDictionary();
    }
}