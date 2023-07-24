using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.TextRenderings;

public class ScriptFontBug : FontDefinitionTest
{
    public ScriptFontBug() : base("Uses Script -- a built in but non truetype font")
    {
    }

    protected override PdfObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("Script"))
            .AsDictionary();
}