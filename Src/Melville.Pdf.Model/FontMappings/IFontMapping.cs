using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.FontMappings;
public interface IFontMapping
{
    object Font { get; }
    IByteToUnicodeMapping Mapping { get; }
    bool Bold { get; }
    bool Oblique { get; }
}

public interface IDefaultFontMapper
{
    public IFontMapping MapDefaultFont(PdfName font, IByteToUnicodeMapping suggestedEncoding);
}
