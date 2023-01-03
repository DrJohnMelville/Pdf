using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class BuiltInTrueType : FontDefinitionTest
{
    public BuiltInTrueType() : base("Uses a TrueType font from the operating system")
    {
    }

    protected override PdfObject CreateFont(ILowLevelDocumentBuilder arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("CooperBlack"))
            .AsDictionary();
}