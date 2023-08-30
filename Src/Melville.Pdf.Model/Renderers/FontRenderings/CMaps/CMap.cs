using Melville.SharpFont;
using Microsoft.CodeAnalysis;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

public record struct CMappingResult(int BytesConsumed, int CharsProduced);

public class CMap
{
    // public CMappingResult Map(scoped in Span<byte> source, scoped in Span<uint> characters)
    // {
    //     
    // }
}