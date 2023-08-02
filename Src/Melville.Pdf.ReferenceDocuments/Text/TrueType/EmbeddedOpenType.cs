
namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class EmbeddedOpenType : FontDefinitionTest
{
    public EmbeddedOpenType() : base("Render with an embedded Opentype font.")
    {
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.GFSEustace.otf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithItem(KnownNames.SubtypeTName, KnownNames.OpenTypeTName)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var widthArray = arg.Add(new PdfArray(Enumerable.Repeat<PdfIndirectObject>(600, 256).ToArray()));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, 32)
            .WithItem(KnownNames.FontBBoxTName, new PdfArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile3TName, stream)
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