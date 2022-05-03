namespace Melville.Pdf.ReferenceDocuments.Text;

public class Type0WithCidToGid : FontDefinitionTest
{
    public Type0WithCidToGid() : base("Type 0 font with CIDToGIDMap")
    {
        TextToRender = "\x0\x4\x0\x5\x0\x6\x0\x7";
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1, new PdfInteger(fontStream.Length))
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
            .WithItem(KnownNames.Flags, new PdfInteger(32))
            .WithItem(KnownNames.FontBBox, new PdfArray(new PdfInteger(-511), new PdfInteger(-250), new PdfInteger(1390), new PdfInteger(750)))
            .WithItem(KnownNames.FontFile2, stream)
            .AsDictionary());
        var sysinfo = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Registry, "Adobe")
            .WithItem(KnownNames.Ordering, "Identity")
            .WithItem(KnownNames.Supplement, 0)
            .AsDictionary()
        );
        var map = arg.Add(new DictionaryBuilder().AsStream(CreateCDIDToGID()));
        var CIDFont = arg.Add(CreateCidFont(descrip, sysinfo, map).AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type0)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("ABCDE+Zev+Regular"))
            .WithItem(KnownNames.Encoding, KnownNames.IdentityH)
            .WithItem(KnownNames.DescendantFonts, new PdfArray(CIDFont))
            .AsDictionary();
    }

    private static DictionaryBuilder CreateCidFont(PdfIndirectReference descrip, PdfIndirectReference sysinfo,
        PdfIndirectReference map)
    {
        var CIDFontBuilder = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
            .WithItem(KnownNames.FontDescriptor, descrip)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("Zev"))
            .WithItem(KnownNames.CIDSystemInfo, sysinfo)
            .WithItem(KnownNames.CIDToGIDMap, map);
        return CIDFontBuilder;
    }

    private byte[] CreateCDIDToGID() => 
        Enumerable.Range(20, 10).SelectMany(i => new byte[] { 0, (byte)i }).ToArray();
}