namespace Melville.Pdf.ReferenceDocuments.Text.CharSet;

public class WinAnsiCharSet : DisplayCharSet
{
    public WinAnsiCharSet() : base(BuiltInFontName.Courier,
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.EncodingTName)
            .WithItem(KnownNames.BaseEncodingTName, KnownNames.WinAnsiEncodingTName)
            .AsDictionary()
        )
    {
    }
}