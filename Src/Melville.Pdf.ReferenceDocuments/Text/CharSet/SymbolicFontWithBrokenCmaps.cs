using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.ReferenceDocuments.Text.CharSet;

public class SymbolicFontWithBrokenCmaps : FontDefinitionTest
{
    public SymbolicFontWithBrokenCmaps() : base("Revert to roman rendering if the CMAPS are broken in a symbolic font.")
    {
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.SymbolicTTFontWithABrokenCMAP.TTF")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream, StreamFormat.DiskRepresentation));
        var widthArray = arg.Add(new PdfArray(Enumerable.Repeat<PdfIndirectObject>(600, 256).ToArray()));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, (int)FontFlags.Symbolic)
            .WithItem(KnownNames.FontBBoxTName, new PdfArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile2TName, stream)
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.FontDescriptorTName, descrip)
            .WithItem(KnownNames.WidthsTName, widthArray)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("Zev"))
            .AsDictionary();
    }
}