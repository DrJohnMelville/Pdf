using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.OtherFontTypes;

public class Type0WithCidToGid : FontDefinitionTest
{
    public Type0WithCidToGid() : base("Type 0 font with CIDToGIDMap")
    {
        TextToRender = "\x0\x4\x0\x5\x0\x6\x0\x7";
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var descrip = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, 32)
            .WithItem(KnownNames.FontBBoxTName, new PdfValueArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile2TName, stream)
            .AsDictionary());
        var sysinfo = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.RegistryTName, "Adobe")
            .WithItem(KnownNames.OrderingTName, "Identity")
            .WithItem(KnownNames.SupplementTName, 0)
            .AsDictionary()
        );
        var map = arg.Add(new ValueDictionaryBuilder().AsStream(CreateCDIDToGID()));
        var CIDFont = arg.Add(CreateCidFont(descrip, sysinfo, map).AsDictionary());
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type0TName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("ABCDE+Zev+Regular"))
            .WithItem(KnownNames.EncodingTName, KnownNames.IdentityHTName)
            .WithItem(KnownNames.DescendantFontsTName, new PdfValueArray(CIDFont))
            .AsDictionary();
    }

    private static ValueDictionaryBuilder CreateCidFont(PdfIndirectValue descrip, PdfIndirectValue sysinfo,
        PdfIndirectValue map)
    {
        var CIDFontBuilder = new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.CIDFontType2TName)
            .WithItem(KnownNames.FontDescriptorTName, descrip)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("Zev"))
            .WithItem(KnownNames.CIDSystemInfoTName, sysinfo)
            .WithItem(KnownNames.CIDToGIDMapTName, map);
        return CIDFontBuilder;
    }

    private byte[] CreateCDIDToGID() =>
        Enumerable.Range(20, 10).SelectMany(i => new byte[] { 0, (byte)i }).ToArray();
}