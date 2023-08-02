
namespace Melville.Pdf.ReferenceDocuments.Text.OtherFontTypes;

public class Type0WithCidToGid : FontDefinitionTest
{
    public Type0WithCidToGid() : base("Type 0 font with CIDToGIDMap")
    {
        TextToRender = "\x0\x4\x0\x5\x0\x6\x0\x7";
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, 32)
            .WithItem(KnownNames.FontBBoxTName, new PdfArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile2TName, stream)
            .AsDictionary());
        var sysinfo = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.RegistryTName, "Adobe")
            .WithItem(KnownNames.OrderingTName, "Identity")
            .WithItem(KnownNames.SupplementTName, 0)
            .AsDictionary()
        );
        var map = arg.Add(new DictionaryBuilder().AsStream(CreateCDIDToGID()));
        var CIDFont = arg.Add(CreateCidFont(descrip, sysinfo, map).AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type0TName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("ABCDE+Zev+Regular"))
            .WithItem(KnownNames.EncodingTName, KnownNames.IdentityHTName)
            .WithItem(KnownNames.DescendantFontsTName, new PdfArray(CIDFont))
            .AsDictionary();
    }

    private static DictionaryBuilder CreateCidFont(PdfIndirectObject descrip, PdfIndirectObject sysinfo,
        PdfIndirectObject map)
    {
        var CIDFontBuilder = new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.CIDFontType2TName)
            .WithItem(KnownNames.FontDescriptorTName, descrip)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("Zev"))
            .WithItem(KnownNames.CIDSystemInfoTName, sysinfo)
            .WithItem(KnownNames.CIDToGIDMapTName, map);
        return CIDFontBuilder;
    }

    private byte[] CreateCDIDToGID() =>
        Enumerable.Range(20, 10).SelectMany(i => new byte[] { 0, (byte)i }).ToArray();
}