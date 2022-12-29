using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.OtherFontTypes;

public class EmbeddedType1 : FontDefinitionTest
{
    public EmbeddedType1() : base("Render with an embedded Type 1 font.")
    {
        TextToRender = "1jn";
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Type1Font.Fon")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var widthArray = arg.Add(new PdfArray(Enumerable.Repeat<PdfObject>(600, 256)));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
            .WithItem(KnownNames.Flags, 32)
            .WithItem(KnownNames.FontBBox, new PdfArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile, stream)
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.FontDescriptor, descrip)
            .WithItem(KnownNames.Widths, widthArray)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("MFSGS-Dingbats"))
            .AsDictionary();
    }
}