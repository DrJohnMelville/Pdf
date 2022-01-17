namespace Melville.Pdf.ReferenceDocuments.Text;

public class WinAnsiCharSet : DisplayCharSet
{
    public WinAnsiCharSet() : base(BuiltInFontName.Courier,
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.BaseEncoding, KnownNames.WinAnsiEncoding)
            .AsDictionary()
        )
    {
    }
}