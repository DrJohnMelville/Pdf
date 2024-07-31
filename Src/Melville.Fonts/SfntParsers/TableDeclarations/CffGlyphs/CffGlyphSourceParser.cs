using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffGlyphSourceParser(
    IMultiplexSource source, ushort unitsPerEm)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseGenericFontAsync()
    {
        using var pipe = source.ReadPipeFrom(0);
        var (headerSize, offsetSize) = await ReadHeaderAsync(pipe).CA();
        await pipe.SkipForwardToAsync(headerSize).CA();
        var nameIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var topIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var stringIndexOffset = pipe.Position;
        var stringIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var globalSubrIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var globalSubroutineExecutor = new GlyphSubroutineExecutor(globalSubrIndex);

        if (topIndex.Length == 1) 
            return await CreateSingleFontAsync(
                topIndex, stringIndexOffset, globalSubroutineExecutor, 0, 
                await GetNameAsync(nameIndex, 0).CA()).CA();

        var ret = new IGenericFont[topIndex.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = await CreateSingleFontAsync(
                topIndex, stringIndexOffset, globalSubroutineExecutor, i, 
                await GetNameAsync(nameIndex, i).CA()).CA();
        }

        return ret;
    }

    private async ValueTask<string> GetNameAsync(CffIndex nameIndex, int item)
    {
        using var bits = await nameIndex.ItemDataAsync(item).CA();
        return Encoding.UTF8.GetString(bits.Sequence);
    }

    private async Task<CffGenericFont> CreateSingleFontAsync(CffIndex topIndex, long stringIndexOffset,
        GlyphSubroutineExecutor globalSubroutineExecutor, int index, string fontName)
    {
        using var firstFontTopDictData = await topIndex.ItemDataAsync(index).CA();
        ParseTopDict(
            firstFontTopDictData.Sequence, out var charStringOffset,
            out var privateOffset, out var privateSize, out var charSetOffset,
            out var encodingOffset);
        var font = new CffGenericFont(source, unitsPerEm,
            fontName,stringIndexOffset, 
            await ReadCharStringIndexAsync(charStringOffset).CA(),
            privateOffset, privateSize, globalSubroutineExecutor, charSetOffset,
            encodingOffset);
        return font;
    }

    private async ValueTask<CffIndex> ReadCharStringIndexAsync(long charStringOffset)
    {
        using var pipe = source.ReadPipeFrom(charStringOffset, charStringOffset);
        var charStringsIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        return charStringsIndex;
    }
 
    //per Adobe Technical note 5716 page 15
    private const int charSetInstruction = 15;
    private const int encodingInstruction = 16;
    private const int charStringsInstruction = 17;
    private const int privateInstruction = 18;
    private static void ParseTopDict(ReadOnlySequence<byte> first, 
        out long charStringOffset, 
        out long privateOffset, 
        out long privateSize, 
        out long charSetOffset,
        out long encodingOffset)
    {
        charStringOffset = privateOffset = privateSize = charSetOffset = encodingOffset = 0;
        Span<DictValue> result = stackalloc DictValue[2];
        var dictParser = new DictParser<CffDictionaryDefinition>(new SequenceReader<byte>(first), result);
        while (dictParser.ReadNextInstruction() is var instr and not 255)
        {
            switch (instr)
            {
                case encodingInstruction:
                    encodingOffset= result[0].IntValue;
                    break;
                case charSetInstruction:
                    charSetOffset = result[0].IntValue;
                    break;
                case charStringsInstruction:
                    charStringOffset = result[0].IntValue;
                    break;
                case privateInstruction:
                    privateSize = result[0].IntValue;
                    privateOffset = result[1].IntValue;
                    break;
            }
        }
    }

    private async ValueTask<(byte headerSize, byte offetSize)> ReadHeaderAsync(IByteSource pipe)
    {
        var results = await pipe.ReadAtLeastAsync(4).CA();
        return ParseHeader(pipe, results.Buffer);
    }

    private (byte headerSize, byte offetSize) ParseHeader(IByteSource pipe, ReadOnlySequence<byte> buffer)
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