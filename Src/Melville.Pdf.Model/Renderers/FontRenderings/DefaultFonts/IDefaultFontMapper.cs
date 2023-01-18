using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

public readonly partial struct DefaultFontReference
{
    [FromConstructor] public Stream Source { get; }
    [FromConstructor] public int Index { get; }
}

public interface IDefaultFontMapper
{
    DefaultFontReference FontFromName(PdfName font, FontFlags flags);
}
