using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class DingBatsWithANameDifferences : FontDefinitionTest
{

    public DingBatsWithANameDifferences() : base("Uses an encoding dictionary To recode dingbats with postscript A names")
    {
        TextToRender = "\u0001\u0002\u0003\u0004";
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var enc = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.Differences, new PdfArray(
                1,
                NameDirectory.Get("a109"),
                NameDirectory.Get("a110"),
                NameDirectory.Get("a111"),
                NameDirectory.Get("a112")
            ))
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, BuiltInFontName.ZapfDingbats)
            .WithItem(KnownNames.Encoding, enc)
            .AsDictionary();
    }
}