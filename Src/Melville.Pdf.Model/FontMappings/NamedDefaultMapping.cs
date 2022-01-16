using Melville.Pdf.LowLevel.Model.CharacterEncoding;

namespace Melville.Pdf.Model.FontMappings;

public record NamedDefaultMapping(object Font, bool Bold, bool Oblique, IByteToUnicodeMapping Mapping) :
    IFontMapping
{
    
}