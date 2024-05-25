using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat12And13Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly ushort reserved;
    [SFntField] private readonly uint length;
    [SFntField] private readonly uint language;
    [SFntField] private readonly uint numGroups;
    [SFntField(nameof(numGroups))] public readonly SequentialMapGroup[] mapGroups;
    public static async ValueTask<ICmapImplementation> ParseAsync(PipeReader input)
    {
        var rec = await FieldParser.ReadFromAsync<CmapFormat12And13Parser>(input).CA();
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

internal readonly struct SequentialMapKey(uint endChar) : IComparable<SequentialMapGroup>
{
    public int CompareTo(SequentialMapGroup other) =>
        endChar.CompareTo(other.endCharCode);
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
        var index = FindContainingRange(character);

        glyph = index < maps.Length ? maps[index].MapGlyph(character, mapping) : 0;
        return true;
    }

    private uint FindContainingRange(uint character) => 
        (uint)(ForcePositive(maps.AsSpan().BinarySearch(new SequentialMapKey(character))));

    private int ForcePositive(int i) => i >= 0 ? i : ~i;
}
