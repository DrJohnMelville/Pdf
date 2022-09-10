using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

public interface IDefaultFontMapper
{
    ValueTask<IRealizedFont> FontFromName(PdfName font, FontFlags flags, FreeTypeFontFactory factory);
}
