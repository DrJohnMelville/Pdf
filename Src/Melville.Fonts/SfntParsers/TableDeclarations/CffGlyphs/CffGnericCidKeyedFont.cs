using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ParserMapping;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

[FromConstructor]
internal partial class CffGnericCidKeyedFont : CffGenericFont
{
    protected override async Task<ICmapImplementation> ReadCmapAsync(ParseMapBookmark? parseMapAncestor)
    {
        var data = new ushort[(int)charStringIndex.Length];
        var target = new MemoryTarget(data.AsMemory());
        await MapCharSetAsync(target).CA();

        return new CidFontCmapImplementation(data);
    }
}

internal class CidFontCmapImplementation(ushort[] data) : ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings()
    {
        for (uint i = 0; i < data.Length; i++)
        {
            yield return (1, data[i], i);
        }
    }

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        glyph = (uint) data.AsSpan().IndexOf((ushort)character);
        return (glyph < uint.MaxValue) && (bytes >= 1);
    }
}