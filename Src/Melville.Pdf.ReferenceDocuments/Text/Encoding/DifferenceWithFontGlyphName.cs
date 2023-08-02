using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class DifferenceWithFontGlyphName : FontDefinitionTest
{

    public DifferenceWithFontGlyphName() : base("Uses an encoding dictionary to make a different font array")
    {
        TextToRender = "Text\x01";
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var enc = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.Differences, new PdfArray(
                1,
                PdfDirectObject.CreateName("H20648")
            ))
            .AsDictionary());
        var fontStream = arg.Add(new DictionaryBuilder()
            .WithFilter(FilterName.FlateDecode)
            .WithItem(KnownNames.Subtype, PdfDirectObject.CreateName("Type1C"))
            .AsStream(GetType().Assembly
                .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.FlateEncodedPiFont.fon")!, StreamFormat.DiskRepresentation));
        var fontDescriptor = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.FontBBox, new PdfArray(-27, -292, 1023, 981))
            .WithItem(KnownNames.FontFile3, fontStream)
            .WithItem(KnownNames.Flags, 4)
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("UKGVJKB+MathmaticaPi-Three"))
            .WithItem(KnownNames.Encoding, enc)
            .WithItem(KnownNames.FirstChar, 1)
            .WithItem(KnownNames.LastChar, 1)
            .WithItem(KnownNames.FontDescriptor, fontDescriptor)
            .WithItem(KnownNames.Widths, new PdfArray(333))
            .AsDictionary();
    }
}