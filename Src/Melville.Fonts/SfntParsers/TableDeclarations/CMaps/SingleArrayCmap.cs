using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal class SingleArrayCmap<T>(int bytesNeeded, uint firstIndex, T[] array) : 
    ICmapImplementation where T : IBinaryInteger<T>, IConvertible
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings() =>
        array
            .Select((t, i) => (bytesNeeded, (uint)(i + firstIndex), t.ToUInt32(null)))
            .Where(i=>i.Item3 > 0);

    public bool TryMap(int bytes, uint character, out uint glyph)
    { 
        var finalIndex = (uint)(character - firstIndex);
        glyph = finalIndex < array.Length ?(uint) array[finalIndex].ToUInt32(null):0;
        return bytes >= bytesNeeded;
    }
}