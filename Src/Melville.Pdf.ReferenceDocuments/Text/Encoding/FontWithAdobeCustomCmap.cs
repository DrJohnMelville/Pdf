using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class FontWithAdobeCustomCmap : FontDefinitionTest
{
    /*
     * I am pretty sure this is a malformed PDF file. The relevant CFF symbolic font has no encoding dictionary. The
     * only place I can find the Charater \0xB6 => Glyph 1 mapping is in a nonstandard Adobe CMAP in the font file. I
     * can find nowhere where the standard mentions, let alone tells me to interpret, this CMAP.  Both the windows
     * and adobe renderes render this file correctly.
     *
     * I came up with a hack that makes this work, and I will invoke Postel's law to defend it. For the single
     * character fonts I scan all the cmaps when I am done. If there is any entry mapping a single byte that is
     * currently unmapped, I use it. Since using an unmapped character is invalid PDF anyway, I can define my error
     * behavior to make a few more PDF files work.
     */
    public FontWithAdobeCustomCmap() : base("This symbol font uses and adobe custom CMAP.  This may be a correct file.")
    {
        TextToRender = "\xB6";
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream(
            "Melville.Pdf.ReferenceDocuments.Text.FlatedFontWithAdobeCmap.ttf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1CTName)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream, StreamFormat.DiskRepresentation));
        var widthArray = arg.Add(new PdfArray(Enumerable.Repeat<PdfIndirectObject>(600, 256).ToArray()));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, 4)
            .WithItem(KnownNames.FontBBoxTName, new PdfArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile3TName, stream)
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.FirstCharTName, 0)
            .WithItem(KnownNames.FontDescriptorTName, descrip)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("JLPYHV+UniversalStd-NewsmithCommPo"))
            .AsDictionary();
    }
}