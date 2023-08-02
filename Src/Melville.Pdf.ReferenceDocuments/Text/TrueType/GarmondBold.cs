using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class GarmondBold : FontDefinitionTest
{
    public GarmondBold() : base("Use a builtin font with bold font descriptor")
    {
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("GarmondBold"))
            .WithItem(KnownNames.FontDescriptorTName,
                arg.Add(new DictionaryBuilder()
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

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectObject.CreateName("GarmondItalic"))
            .WithItem(KnownNames.FontDescriptorTName,
                arg.Add(new DictionaryBuilder()
                    .WithItem(KnownNames.FlagsTName, (long)FontFlags.Italic)
                    .AsDictionary())
                )
            .AsDictionary();
}