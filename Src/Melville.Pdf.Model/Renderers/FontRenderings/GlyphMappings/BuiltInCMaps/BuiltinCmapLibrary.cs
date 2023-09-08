using System.IO;
using System.IO.Compression;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Microsoft.VisualBasic;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings.BuiltInCMaps;

[StaticSingleton]
internal partial class BuiltinCmapLibrary: IRetrieveCmapStream
{
    public Stream CMapStreamFor(PdfDirectObject name)
    {
        var type = typeof(BuiltinCmapLibrary);
        return new BrotliStream(type.Assembly.GetManifestResourceStream(type, name.ToString()) ??
                                 throw new PdfParseException("Unknown built in CMAP name."),
            CompressionMode.Decompress);
    }
}