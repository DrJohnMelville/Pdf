using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;

public class FontFamily
{
    public FontFamily(string familyName)
    {
        FamilyName = familyName;
    }

    public string FamilyName { get; }
    public FontReference? Normal { get; set; }
    public FontReference? Bold { get; set; }
    public FontReference? Italic { get; set; }
    public FontReference? BoldItalic { get; set; }

    public void Register(FontReference fontReference, bool isBold, bool isItalic)
    {
        switch (isBold, isItalic)
        {
            case (true, true): BoldItalic ??= fontReference; break;
            case (true, false): Bold ??= fontReference; break;
            case (false, true): Italic ??= fontReference; break;
            default: Normal ??= fontReference; break;
        }
    }

    public FontReference SelectFace(bool bold, bool italic) =>
        (bold, italic) switch
        {
            (true, true) => BoldItalic ?? Bold ?? Italic ?? Normal,
            (false, true) => Italic ?? BoldItalic ?? Normal ?? Bold,
            (true, false) => Bold ?? BoldItalic ?? Normal ?? Italic,
            _ => Normal ?? Italic ?? Bold ?? BoldItalic
        } ?? throw new PdfParseException("Should not have a FontFamily with 4 null members");
}