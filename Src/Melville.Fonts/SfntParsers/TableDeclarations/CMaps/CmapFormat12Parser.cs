using System.IO.Pipelines;
using System.Windows.Markup;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat12Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly ushort reserved;
    [SFntField] private readonly uint length;
    [SFntField] private readonly uint language;
    [SFntField] private readonly uint numGroups;
    [SFntField(nameof(numGroups))] public readonly SequentialMapGroup[] mapGroups;
    public static async ValueTask<ICmapImplementation> ParseAsync(PipeReader input)
    {
        var rec = await FieldParser.ReadFromAsync<CmapFormat12Parser>(input).CA();
        return new CmapFormat12or13Implementation(rec.mapGroups,
            rec.format == 13?Map13:Map12);
    }
     public static uint Map12(uint character, uint startChar, uint startGlyph) => 
         character - startChar + startGlyph;
     public static uint Map13(uint character, uint startChar, uint startGlyph) => startGlyph;
}

internal delegate uint Type12Or13MappingStrategy(uint character, uint startChar, uint startGlyph);

internal readonly partial struct SequentialMapGroup
{
    [SFntField] public readonly uint startCharCode;
    [SFntField] public readonly uint endCharCode;
    [SFntField] private readonly uint startGlyphId;
    public uint MapGlyph(uint character, Type12Or13MappingStrategy mapping) => 
        character < startCharCode || character > endCharCode? 0:
        mapping(character, startCharCode, startGlyphId);
}

internal class CmapFormat12or13Implementation (
    SequentialMapGroup[] maps, Type12Or13MappingStrategy mapping) : ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings()
    {
        foreach (var map in maps)
        {
            for (uint i = map.startCharCode; i <= map.endCharCode; i++)
            {
                yield return new (4, i, map.MapGlyph(i, mapping));
            }
        }
    }

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        if (bytes < 4) {glyph = 0; return false;}

        var range = maps.AsSpan();
        while (true)
        {
            switch (range.Length)
            {
                case 0:
                    return ReturnGlyph(out glyph, 0);
                case 1:
                    return ReturnGlyph(out glyph, range[0].MapGlyph(character, mapping));
                case 2:
                    BinarySplit(character, ref range, 0);
                    break;
                default:
                    BinarySplit(character, ref range, range.Length / 2);
                    break;
            }
        }
    }

    private static void BinarySplit(uint character,  ref Span<SequentialMapGroup> range, int midPoint)
    {
        var mid = midPoint;
        range = character > range[mid].endCharCode ? 
            range[(mid+1)..] : range[..(mid+1)];
    }

    private static bool ReturnGlyph(out uint glyph, uint result)
    {
        glyph = result;
        return true;
    }
}
