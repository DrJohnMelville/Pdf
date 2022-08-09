using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text;

public class EmbeddedTrueType : FontDefinitionTest
{
    public EmbeddedTrueType() : base("Render with an embedded truetype font.")
    {
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1, new PdfInteger(fontStream.Length))
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var widthArray = arg.Add(new PdfArray(Enumerable.Repeat<PdfObject>(new PdfInteger(600), 256)));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
            .WithItem(KnownNames.Flags, new PdfInteger(32))
            .WithItem(KnownNames.FontBBox, new PdfArray(new PdfInteger(-511), new PdfInteger(-250), new PdfInteger(1390), new PdfInteger(750)))
            .WithItem(KnownNames.FontFile2, stream)
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.FontDescriptor, descrip)
            .WithItem(KnownNames.Widths, widthArray)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("Zev"))
            .AsDictionary();
    }
}