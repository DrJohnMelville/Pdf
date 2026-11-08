using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;

using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal readonly partial struct CMapStreamParser
{
    private readonly List<uint> dictionary = new();
    [FromConstructor] private readonly ParsingBuffer pipe;
    
    public async ValueTask<IMapCharacterToGlyph> ParseAsync()
    {
        while (await pipe.TryEnsureBytesAsync(2).CA())
        {
            dictionary.Add(pipe.GetUInt16BigEndian());
        }
        return new CharacterToGlyphArray(dictionary);
    }
}
