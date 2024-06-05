using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffGlyphSourceParser(IMultiplexSource source)
{
    public async Task<IGlyphSource> ParseAsync()
    {
        var pipe = new ByteSource(source.ReadPipeFrom(0));
        var (headerSize, offsetSize) = await ReadHeaderAsync(pipe).CA();
        await pipe.SkipForwardToAsync(headerSize).CA();
        var nameIndex = await new CFFIndexParser(source, pipe).ParseAsync().CA();
        var topIndex = await new CFFIndexParser(source, pipe).ParseAsync().CA();
        var stringIndex = await new CFFIndexParser(source, pipe).ParseAsync().CA();
        var globalSubrIndex = await new CFFIndexParser(source, pipe).ParseAsync().CA();


        var firstFontTopDictData = await topIndex.ItemDataAsync(0).CA();
        var offsetVal = GetCharStringOffset(firstFontTopDictData);
        
        await pipe.AdvanceToLocalPositionAsync(offsetVal).CA();
        var charStringsIndex= await new CFFIndexParser(source, pipe).ParseAsync().CA();
        

        return new CffGlyphSource(charStringsIndex);
    }

    private static int GetCharStringOffset(ReadOnlySequence<byte> first)
    {
        var result = new DictValue[1];
        if (!new DictParser<CffDictionaryDefinition>(new SequenceReader<byte>(first), result).TryFindEntry(17))
            throw new InvalidDataException("No charstringoffset for font");
        var offsetVal = result[0].IntValue;
        return offsetVal;
    }

    private async ValueTask<(byte headerSize, byte offetSize)> ReadHeaderAsync(ByteSource pipe)
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
}