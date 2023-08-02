
namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class BuiltInTrueType : FontDefinitionTest
{
    public BuiltInTrueType() : base("Uses a TrueType font from the operating system")
    {
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("CooperBlack"))
            .AsDictionary();
}