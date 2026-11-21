using System;
using System.IO;
using System.IO.Compression;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Streams;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings.BuiltInCMaps;

[StaticSingleton]
internal partial class BuiltinCmapLibrary: IRetrieveCmapStream
{
    public IAsyncReader? CMapStreamFor(PdfDirectObject name) => 
        ResourceForName(name) is { } s ? new BrotliStream(s, CompressionMode.Decompress).AsStreamReader() : null;

    private static Stream? ResourceForName(PdfDirectObject name)
    {
        var type = typeof(BuiltinCmapLibrary);
        return type.Assembly.GetManifestResourceStream(type, name.ToString());
    }
}