using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat10Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly ushort reserved;
    [SFntField] private readonly uint length;
    [SFntField] private readonly uint language;
    [SFntField] private readonly uint startCharCode;
    [SFntField] private readonly uint numChars;
    [SFntField("numChars")] private readonly ushort[] glyphIndexArray;

    public static async ValueTask<ICmapImplementation> ParseAsync(IByteSource input)
    {
        var record = await FieldParser.ReadFromAsync<CmapFormat10Parser>(input).CA();
        return new CmapFormat10Implementation(
            record.startCharCode, record.glyphIndexArray);
    }
}

internal class CmapFormat10Implementation(uint start, ushort[] glyphs) : ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings()
    {
        for (int i = 0; i < glyphs.Length; i++)
        {
            yield return (4, (uint)(start + i), glyphs[i]);
        }
    }

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        if (bytes < 4) {glyph = 0; return false;}

        var offser = (uint)(character - start);
        glyph = MapOffsetToGlyph(offser);
        return true;
    }

    private uint MapOffsetToGlyph(uint offser)
    {
        return offser < glyphs.Length ? glyphs[offser] : 0u;
    }
}