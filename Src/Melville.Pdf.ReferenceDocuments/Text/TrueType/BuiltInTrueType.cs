using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class BuiltInTrueType : FontDefinitionTest
{
    public BuiltInTrueType() : base("Uses a TrueType font from the operating system")
    {
    }

    protected override PdfObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("CooperBlack"))
            .AsDictionary();
}