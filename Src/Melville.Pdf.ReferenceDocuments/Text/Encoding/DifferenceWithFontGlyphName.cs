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
            .WithItem(KnownNames.TypeTName, KnownNames.EncodingTName)
            .WithItem(KnownNames.DifferencesTName, new PdfArray(
                1,
                PdfDirectObject.CreateName("H20648")
            ))
            .AsDictionary());
        var fontStream = arg.Add(new DictionaryBuilder()
            .WithFilter(FilterName.FlateDecode)
            .WithItem(KnownNames.SubtypeTName, PdfDirectObject.CreateName("Type1C"))
            .AsStream(GetType().Assembly
                .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.FlateEncodedPiFont.fon")!, StreamFormat.DiskRepresentation));
        var fontDescriptor = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.FontBBoxTName, new PdfArray(-27, -292, 1023, 981))
            .WithItem(KnownNames.FontFile3TName, fontStream)
            .WithItem(KnownNames.FlagsTName, 4)
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("UKGVJKB+MathmaticaPi-Three"))
            .WithItem(KnownNames.EncodingTName, enc)
            .WithItem(KnownNames.FirstCharTName, 1)
            .WithItem(KnownNames.LastCharTName, 1)
            .WithItem(KnownNames.FontDescriptorTName, fontDescriptor)
            .WithItem(KnownNames.WidthsTName, new PdfArray(333))
            .AsDictionary();
    }
}