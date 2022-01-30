using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

public interface IDefaultFontMapper
{
    public ValueTask<IRealizedFont> MapDefaultFont(PdfName font, double size);
}
