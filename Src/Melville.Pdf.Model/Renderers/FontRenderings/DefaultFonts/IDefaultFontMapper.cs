using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

public interface IDefaultFontMapper
{
    public ValueTask<IRealizedFont> MapDefaultFont(PdfName font, FreeTypeFontFactory factory);
}
