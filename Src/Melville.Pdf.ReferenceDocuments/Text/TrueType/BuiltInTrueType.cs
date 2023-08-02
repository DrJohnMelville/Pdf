
namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class BuiltInTrueType : FontDefinitionTest
{
    public BuiltInTrueType() : base("Uses a TrueType font from the operating system")
    {
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("CooperBlack"))
            .AsDictionary();
}