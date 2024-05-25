using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal class CmapFormat14Implementation (
    VariantSelection[] variants): ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings() => 
        variants.SelectMany(i=>i.AllMappings());

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        if (bytes == 4 && FindVariant(character) is var index and >= 0)
        {
            glyph = variants[index].TryMap(character >> 16);
            return true;
        }

        glyph = 0;
        return true;
    }

    private int FindVariant(uint character) => 
        variants.AsSpan().BinarySearch(
            new VariantSelection.SearchKey((uint)(character&0xFFFF)));
}

internal readonly partial struct VariantSelection
{

    [FromConstructor] private readonly uint selector;
    [FromConstructor] private readonly UvsDefaultMapping[] defaults;
    [FromConstructor] private readonly UvsMapping[] mappings;

    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings()
    {
        var capturedSelector = selector;
        return defaults.SelectMany(i => i.AllMappings(capturedSelector)).Concat(
            mappings.Select(i => i.Mapping(capturedSelector)));
    }

    public struct SearchKey(uint selector): IComparable<VariantSelection>
    {
        public int CompareTo(VariantSelection other) => selector.CompareTo(other.selector);
    }

    public uint TryMap(uint character) => 
        TryLookupDefaultMapping(character) ?? TryLookupNonDefaultMapping(character);

    private uint? TryLookupDefaultMapping(uint character)
    {
        var index = defaults.AsSpan().BinarySearch(new UvsDefaultMapping.SearckKey(character));
        index = index < 0 ? ~index : index;
        return index < defaults.Length ? character : (uint?)null;
    }

    private uint TryLookupNonDefaultMapping(uint character)
    {
        var index = mappings.AsSpan().BinarySearch(new UvsMapping.SearckKey(character));
        return index < mappings.Length ? mappings[index].glyphId : (uint)0;
    }
}

internal readonly partial struct UvsDefaultMapping
{
    [SFntField] private readonly UInt24 unicodeValue;
    [SFntField] private readonly byte additionalCount;

    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings(uint selector)
    {
        return Enumerable.Range(unicodeValue, additionalCount + 1)
            .Select(i => (4, (uint)(i<<16) | selector , (uint)i));
    }

    public readonly struct SearckKey(uint unicodeValue): IComparable<UvsDefaultMapping>
    {
        public int CompareTo(UvsDefaultMapping other) => 
            (unicodeValue).CompareTo(other.unicodeValue+other.additionalCount+1);
    }

    public bool RangeIncludes(uint character) => 
        ((uint)(character - unicodeValue)) <= additionalCount;
}

internal readonly partial struct UvsMapping
{
    [SFntField] public readonly UInt24 unicodeValue;
    [SFntField] public readonly ushort glyphId;

    public (int Bytes, uint Character, uint Glyph) Mapping(uint selector) => 
        (4, (uint)(unicodeValue<<16)|selector, glyphId);

    public readonly struct SearckKey(uint character) : IComparable<UvsMapping>
    {
        public int CompareTo(UvsMapping other) =>
            character.CompareTo(other.unicodeValue);
    }
}