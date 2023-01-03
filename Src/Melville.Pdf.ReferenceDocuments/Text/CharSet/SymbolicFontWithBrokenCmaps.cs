using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.ReferenceDocuments.Text.CharSet;

public class SymbolicFontWithBrokenCmaps : FontDefinitionTest
{
    public SymbolicFontWithBrokenCmaps() : base("Revert to roman rendering if the CMAPS are broken in a symbolic font.")
    {
    }

    protected override PdfObject CreateFont(IPdfObjectRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.SymbolicTTFontWithABrokenCMAP.TTF")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream, StreamFormat.DiskRepresentation));
        var widthArray = arg.Add(new PdfArray(Enumerable.Repeat<PdfObject>(600, 256)));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
            .WithItem(KnownNames.Flags, (int)FontFlags.Symbolic)
            .WithItem(KnownNames.FontBBox, new PdfArray(-511, -250, 1390, 750))
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