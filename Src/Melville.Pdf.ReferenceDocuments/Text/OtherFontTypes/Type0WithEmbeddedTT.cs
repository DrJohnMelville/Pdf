
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

    protected override DictionaryBuilder CidFontBuilder(IPdfObjectCreatorRegistry arg)
    {
        return base.CidFontBuilder(arg).WithItem(KnownNames.DWTName, 500);
    }
}
public class Type0WithIndividualCharSpacing : Type0Base
{
    public Type0WithIndividualCharSpacing() : base("Type 0 with a first parameter type, which should be individual widths.")
    {
    }

    protected override DictionaryBuilder CidFontBuilder(IPdfObjectCreatorRegistry arg)
    {
        return base.CidFontBuilder(arg).WithItem(KnownNames.WTName, new PdfArray(
            4, new PdfArray(
                500, 750, 250)));
    }
}

public abstract class Type0Base : FontDefinitionTest
{
    public Type0Base(string description) : base(description)
    {
        TextToRender = "\x0\x4\x0\x5\x0\x6\x0\x7";
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var CIDFont = arg.Add(CidFontBuilder(arg).AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type0TName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("ABCDE+Zev+Regular"))
            .WithItem(KnownNames.EncodingTName, KnownNames.IdentityHTName)
            .WithItem(KnownNames.DescendantFontsTName, new PdfArray(CIDFont))
            .AsDictionary();
    }

    protected virtual DictionaryBuilder CidFontBuilder(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, 32)
            .WithItem(KnownNames.FontBBoxTName,
                new PdfArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile2TName, stream)
            .AsDictionary());
        var sysinfo = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.RegistryTName, "Adobe")
            .WithItem(KnownNames.OrderingTName, "Identity")
            .WithItem(KnownNames.SupplementTName, 0)
            .AsDictionary()
        );
        var CIDFontBuilder = new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.CIDFontType2TName)
            .WithItem(KnownNames.FontDescriptorTName, descrip)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("Zev"))
            .WithItem(KnownNames.CIDSystemInfoTName, sysinfo);
        return CIDFontBuilder;
    }
}
