using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.TextRenderings;

public class ScriptFontBug : FontDefinitionTest
{
    public ScriptFontBug() : base("Uses Script -- a built in but non truetype font")
    {
    }

    protected override PdfObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("Script"))
            .AsDictionary();
}