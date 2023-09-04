using System.IO;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings.BuiltInCMaps;

internal static class BuiltinCmapLibrary
{
    public static Stream BuiltinCmap(PdfDirectObject name)
    {
        var type = typeof(BuiltinCmapLibrary);
        return type.Assembly.GetManifestResourceStream(type, $"{name.ToString().ToLower()}.txt") ??
               throw new PdfParseException("Unknown built in CMAP name.");
    }
}