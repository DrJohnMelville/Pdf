using Melville.Pdf.LowLevel.Model.Primitives;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class EmbeddedOpenType : FontDefinitionTest
{
    public EmbeddedOpenType() : base("Render with an embedded Opentype font.")
    {
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.GFSEustace.otf")!;
        var stream = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.Length1TName, fontStream.Length)
            .WithItem(KnownNames.SubtypeTName, KnownNames.OpenTypeTName)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var widthArray = arg.Add(new PdfValueArray(Enumerable.Repeat<PdfIndirectValue>(600, 256).ToArray()));
        var descrip = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontDescriptorTName)
            .WithItem(KnownNames.FlagsTName, 32)
            .WithItem(KnownNames.FontBBoxTName, new PdfValueArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile3TName, stream)
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