using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.CharSet;

public class WinAnsiCharSet : DisplayCharSet
{
    public WinAnsiCharSet() : base(BuiltInFontName.Courier,
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.EncodingTName)
            .WithItem(KnownNames.BaseEncodingTName, KnownNames.WinAnsiEncodingTName)
            .AsDictionary()
        )
    {
    }
}