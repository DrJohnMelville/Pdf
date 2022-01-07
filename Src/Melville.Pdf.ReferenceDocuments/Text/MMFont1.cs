namespace Melville.Pdf.ReferenceDocuments.Text;

/// <summary>
/// This class renders different in my renderer than in the standard one, because Zapf Dingbats is not a real
/// muiltimaster type 1 font.  As of 1/7/2022 type 1 fonts are obsolete, and I am not going to implement them
/// my workarround is just the chop off the multimaster information and use the base font name, which might be
/// a built in font (like this test) or might be a built in font from the operating system.  Otherwise a default
/// font will eventually be substituted.
/// </summary>
public class MMFont1 : FontDefinitionTest
{
    public MMFont1() : base("Uses a Type 1 MultiMaster Type")
    {
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.MMType1)
            .WithItem(KnownNames.BaseFont, NameDirectory.Get("ZapfDingbats_366_465_11_"))
            .AsDictionary();
}