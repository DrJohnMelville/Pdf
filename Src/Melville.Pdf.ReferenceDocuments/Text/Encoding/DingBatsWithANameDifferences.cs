
namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class DingBatsWithANameDifferences : FontDefinitionTest
{

    public DingBatsWithANameDifferences() : base("Uses an encoding dictionary To recode dingbats with postscript A names")
    {
        TextToRender = "\u0001\u0002\u0003\u0004";
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var enc = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.EncodingTName)
            .WithItem(KnownNames.DifferencesTName, new PdfArray(
                1,
                PdfDirectObject.CreateName("a109"),
                PdfDirectObject.CreateName("a110"),
                PdfDirectObject.CreateName("a111"),
                PdfDirectObject.CreateName("a112")
            ))
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, (PdfDirectObject)BuiltInFontName.ZapfDingbats)
            .WithItem(KnownNames.EncodingTName, enc)
            .AsDictionary();
    }
}