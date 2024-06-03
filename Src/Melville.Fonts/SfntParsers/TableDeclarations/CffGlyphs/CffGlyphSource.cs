using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;


namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal class CFFGlyphSource : IGlyphSource
{
    private readonly CffIndex glyphs;
    internal CFFGlyphSource(CffIndex glyphs) => this.glyphs = glyphs;

    public int GlyphCount => (int)glyphs.Length;
}

public readonly struct CffGlyphSourceParser(IMultiplexSource source)
{
    public async Task<IGlyphSource> ParseAsync()
    {
        var pipe = new ByteSource(source.ReadPipeFrom(0));
        var (headerSize, offsetSize) = await ReadHeader(pipe).CA();
        await pipe.SkipForwardToAsync(headerSize).CA();
        await ReadNameIndexAsync(pipe).CA();
        var topIndex = await new CFFIndexParser(source, pipe).ParseAsync().CA();
        var first = await topIndex.ItemDataAsync(0).CA();
        var offsetVal = GetCharStrinOffset(first);

        await pipe.AdvanceToLocalPositionAsync(offsetVal).CA();
        return new CFFGlyphSource(await new CFFIndexParser(source, pipe).ParseAsync().CA());
    }

    private static int GetCharStrinOffset(ReadOnlySequence<byte> first)
    {
        var result = new DictValue[1];
        if (!new DictScanner(new SequenceReader<byte>(first), 17, result).TryFindEntry())
            throw new InvalidDataException("No charstringoffset for font");
        var offsetVal = result[0].IntValue;
        return offsetVal;
    }

    private async ValueTask<(byte headerSize, byte offetSize)> ReadHeader(ByteSource pipe)
    {
        var results = await pipe.ReadAtLeastAsync(4).CA();
        return ParseHeader(pipe, results.Buffer);
    }

    private (byte headerSize, byte offetSize) ParseHeader(ByteSource pipe, ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        var majorVersion = reader.ReadBigEndianUint8();
        var minorVersion = reader.ReadBigEndianUint8();
        var headerSize = reader.ReadBigEndianUint8();
        var offSize = reader.ReadBigEndianUint8();

        pipe.AdvanceTo(reader.Position);
        Debug.Assert(majorVersion == 1);
        Debug.Assert(minorVersion == 0);
        return (headerSize, offSize);
    }

    private async ValueTask ReadNameIndexAsync(ByteSource pipe)
    {
        #warning implement a skip index method since I don't want this index.
        var nameIndex = await new CFFIndexParser(source, pipe).ParseAsync().CA();
    }
}

public class CffGlyphSource(): IGlyphSource
{
    public int GlyphCount => throw new NotImplementedException();
}