using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.OtherFontTypes;

public class Type0WithEmbeddedTT : Type0Base
{
    public Type0WithEmbeddedTT() : base("Render with an simple type 0 font.")
    {
    }
}

public class Type0WithTightDefaultCharSpacing : Type0Base
{
    public Type0WithTightDefaultCharSpacing() : base("Type 0 with a DW parameter of 500, which should be tight chars.")
    {
    }

    protected override DictionaryBuilder CidFontBuilder(ILowLevelDocumentCreator arg)
    {
        return base.CidFontBuilder(arg).WithItem(KnownNames.DW, 500);
    }
}
public class Type0WithIndividualCharSpacing : Type0Base
{
    public Type0WithIndividualCharSpacing() : base("Type 0 with a first parameter type, which should be individual widths.")
    {
    }

    protected override DictionaryBuilder CidFontBuilder(ILowLevelDocumentCreator arg)
    {
        return base.CidFontBuilder(arg).WithItem(KnownNames.W, new PdfArray(
            new PdfInteger(4), new PdfArray(
                new PdfInteger(500), new PdfInteger(750), new PdfInteger(250))));
    }
}

public abstract class Type0Base : FontDefinitionTest
{
    public Type0Base(string description) : base(description)
    {
        TextToRender = "\x0\x4\x0\x5\x0\x6\x0\x7";
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var CIDFont = arg.Add(CidFontBuilder(arg).AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type0)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("ABCDE+Zev+Regular"))
            .WithItem(KnownNames.Encoding, KnownNames.IdentityH)
            .WithItem(KnownNames.DescendantFonts, new PdfArray(CIDFont))
            .AsDictionary();
    }

    protected virtual DictionaryBuilder CidFontBuilder(ILowLevelDocumentCreator arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1, new PdfInteger(fontStream.Length))
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
            .WithItem(KnownNames.Flags, new PdfInteger(32))
            .WithItem(KnownNames.FontBBox,
                new PdfArray(new PdfInteger(-511), new PdfInteger(-250), new PdfInteger(1390), new PdfInteger(750)))
            .WithItem(KnownNames.FontFile2, stream)
            .AsDictionary());
        var sysinfo = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Registry, "Adobe")
            .WithItem(KnownNames.Ordering, "Identity")
            .WithItem(KnownNames.Supplement, 0)
            .AsDictionary()
        );
        var CIDFontBuilder = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
            .WithItem(KnownNames.FontDescriptor, descrip)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("Zev"))
            .WithItem(KnownNames.CIDSystemInfo, sysinfo);
        return CIDFontBuilder;
    }
}
