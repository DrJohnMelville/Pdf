using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class GarmondBold : FontDefinitionTest
{
    public GarmondBold() : base("Use a builtin font with bold font descriptor")
    {
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("GarmondBold"))
            .WithItem(KnownNames.FontDescriptorTName,
                arg.Add(new ValueDictionaryBuilder()
                    .WithItem(KnownNames.FlagsTName, (long)FontFlags.ForceBold)
                    .AsDictionary())
                )
            .AsDictionary();
}
public class GarmondItalic : FontDefinitionTest
{
    public GarmondItalic() : base("Use builtin font with italic descriptor")
    {
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("GarmondItalic"))
            .WithItem(KnownNames.FontDescriptorTName,
                arg.Add(new ValueDictionaryBuilder()
                    .WithItem(KnownNames.FlagsTName, (long)FontFlags.Italic)
                    .AsDictionary())
                )
            .AsDictionary();
}