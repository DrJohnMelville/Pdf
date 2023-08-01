using Melville.Pdf.LowLevel.Model.Primitives;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.ReferenceDocuments.Text.TextRenderings;

public class ScriptFontBug : FontDefinitionTest
{
    public ScriptFontBug() : base("Uses Script -- a built in but non truetype font")
    {
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.TrueTypeTName)
            .WithItem(KnownNames.BaseFontTName, PdfDirectValue.CreateName("Script"))
            .AsDictionary();
}