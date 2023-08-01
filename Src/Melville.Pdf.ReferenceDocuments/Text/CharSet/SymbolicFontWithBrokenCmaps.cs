using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.ReferenceDocuments.Text.CharSet;

public class SymbolicFontWithBrokenCmaps : FontDefinitionTest
{
    public SymbolicFontWithBrokenCmaps() : base("Revert to roman rendering if the CMAPS are broken in a symbolic font.")
    {
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.SymbolicTTFontWithABrokenCMAP.TTF")!;
        var stream = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream, StreamFormat.DiskRepresentation));
        var widthArray = arg.Add(new PdfValueArray(Enumerable.Repeat<PdfIndirectValue>(600, 256).ToArray()));
        var descrip = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, (int)FontFlags.Symbolic)
            .WithItem(KnownNames.FontBBoxTName, new PdfValueArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile2TName, stream)
            .AsDictionary());
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.FontDescriptorTName, descrip)
            .WithItem(KnownNames.WidthsTName, widthArray)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("Zev"))
            .AsDictionary();
    }
}