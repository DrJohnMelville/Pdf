using Melville.Pdf.LowLevel.Model.Objects2;
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

    protected override ValueDictionaryBuilder CidFontBuilder(IPdfObjectCreatorRegistry arg)
    {
        return base.CidFontBuilder(arg).WithItem(KnownNames.DWTName, 500);
    }
}
public class Type0WithIndividualCharSpacing : Type0Base
{
    public Type0WithIndividualCharSpacing() : base("Type 0 with a first parameter type, which should be individual widths.")
    {
    }

    protected override ValueDictionaryBuilder CidFontBuilder(IPdfObjectCreatorRegistry arg)
    {
        return base.CidFontBuilder(arg).WithItem(KnownNames.WTName, new PdfValueArray(
            4, new PdfValueArray(
                500, 750, 250)));
    }
}

public abstract class Type0Base : FontDefinitionTest
{
    public Type0Base(string description) : base(description)
    {
        TextToRender = "\x0\x4\x0\x5\x0\x6\x0\x7";
    }

    protected override PdfObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var CIDFont = arg.Add(CidFontBuilder(arg).AsDictionary());
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type0TName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("ABCDE+Zev+Regular"))
            .WithItem(KnownNames.EncodingTName, KnownNames.IdentityHTName)
            .WithItem(KnownNames.DescendantFontsTName, new PdfValueArray(CIDFont))
            .AsDictionary();
    }

    protected virtual ValueDictionaryBuilder CidFontBuilder(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var descrip = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, 32)
            .WithItem(KnownNames.FontBBoxTName,
                new PdfValueArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile2TName, stream)
            .AsDictionary());
        var sysinfo = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.RegistryTName, "Adobe")
            .WithItem(KnownNames.OrderingTName, "Identity")
            .WithItem(KnownNames.SupplementTName, 0)
            .AsDictionary()
        );
        var CIDFontBuilder = new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.CIDFontType2TName)
            .WithItem(KnownNames.FontDescriptorTName, descrip)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("Zev"))
            .WithItem(KnownNames.CIDSystemInfoTName, sysinfo);
        return CIDFontBuilder;
    }
}
