
namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class DingBatsWithANameDifferences : FontDefinitionTest
{

    public DingBatsWithANameDifferences() : base("Uses an encoding dictionary To recode dingbats with postscript A names")
    {
        TextToRender = "\u0001\u0002\u0003\u0004";
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var enc = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.EncodingTName)
            .WithItem(KnownNames.DifferencesTName, new PdfValueArray(
                1,
                PdfDirectValue.CreateName("a109"),
                PdfDirectValue.CreateName("a110"),
                PdfDirectValue.CreateName("a111"),
                PdfDirectValue.CreateName("a112")
            ))
            .AsDictionary());
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, (PdfDirectValue)BuiltInFontName.ZapfDingbats)
            .WithItem(KnownNames.EncodingTName, enc)
            .AsDictionary();
    }
}