using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal interface ICharSetTarget
{
    public long Count { get; }
    ValueTask SetGlyphNameAsync(int index, ushort SID);
}

internal readonly struct StringTarget(CffStringIndex strings, long numGlyphs) : ICharSetTarget
{
    public string[] Result { get; } = CreateResult(numGlyphs);

    private static string[] CreateResult(long numGlyphs)
    {
        var ret = new string[numGlyphs];
        ret[0] = ".notdef";
        return ret;
    }

    public long Count => numGlyphs;

    public async ValueTask SetGlyphNameAsync(int index, ushort sid) =>
        Result[index] = await strings.GetNameAsync(sid).CA();
}

internal readonly struct MemoryTarget(Memory<ushort> target) : ICharSetTarget
{
    public Memory<ushort> Target => target;
    public long Count => target.Length;

    public ValueTask SetGlyphNameAsync(int index, ushort sid)
    {
        if ((uint)index < target.Length) target.Span[index] = sid;
        return default;
    }
}

internal readonly struct CharSetReader<T>(
    IByteSource charsetPipe,
    T target) where T : ICharSetTarget
{
    public async ValueTask<T> ReadCharSetAsync()
    {
        var type = await charsetPipe.ReadBigEndianUintAsync(1).CA();
        switch (type)
        {
            case 0:
                await ParseType0SetAsync().CA();
                break;
            case 1:
                await ParseType1or2SetAsync(1).CA();
                break;
            case 2:
                await ParseType1or2SetAsync(2).CA();
                break;
            default:
                throw new InvalidDataException("Invalid charset type");
        }

        return target;
    }

    private async ValueTask ParseType0SetAsync()
    {
        for (int i = 1; i < target.Count; i++)
        {
            await target.SetGlyphNameAsync(i,
                (ushort)await charsetPipe.ReadBigEndianUintAsync(2).CA()).CA();
        }
    }

    private async ValueTask ParseType1or2SetAsync(int nLength)
    {
        int position = 1;
        while (position < target.Count)
        {
            var first = (int)await charsetPipe.ReadBigEndianUintAsync(2).CA();
            var count = (int)await charsetPipe.ReadBigEndianUintAsync(nLength).CA();
            for (int i = 0; i < count + 1; i++)
            {
                await target.SetGlyphNameAsync(position++, (ushort)(first + i)).CA();
            }
        }
    }
}