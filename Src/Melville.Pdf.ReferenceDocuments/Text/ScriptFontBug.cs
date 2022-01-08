namespace Melville.Pdf.ReferenceDocuments.Text;

public class ScriptFontBug : FontDefinitionTest
{
    public ScriptFontBug() : base("Uses Script -- a built in but non truetype font")
    {
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("Script"))
            .AsDictionary();
}