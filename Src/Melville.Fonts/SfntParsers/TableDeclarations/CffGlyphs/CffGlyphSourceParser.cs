using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffGlyphSourceParser(
    IMultiplexSource source, ushort unitsPerEm)
{
    public async Task<IGlyphSource> ParseAsync()
    {
        var pipe = new ByteSource(source.ReadPipeFrom(0));
        var (headerSize, offsetSize) = await ReadHeaderAsync(pipe).CA();
        await pipe.SkipForwardToAsync(headerSize).CA();
        var nameIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var topIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var stringIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        var globalSubrIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();


        var firstFontTopDictData = await topIndex.ItemDataAsync(0).CA();
        ParseTopDict(
            firstFontTopDictData, out var charStringOffset,
            out var privateOffset, out var privateSize);
        
        await pipe.AdvanceToLocalPositionAsync(charStringOffset).CA();
        var charStringsIndex= await new CFFIndexParser(source, pipe).ParseCff1Async().CA();

        var privateSubrs = await GetrivateSubrsAsync(pipe, privateOffset, privateSize).CA();

        return new CffGlyphSource(charStringsIndex, 
            new GlyphSubroutineExecutor(globalSubrIndex), 
            new GlyphSubroutineExecutor(privateSubrs), Matrix3x2.CreateScale(1f/unitsPerEm));
    }

    private async ValueTask<CffIndex> GetrivateSubrsAsync(ByteSource pipe, int privateOffset, int privateSize)
    {
        await pipe.AdvanceToLocalPositionAsync(privateOffset).CA();
        var privateDictBytes = await pipe.ReadAtLeastAsync(privateSize).CA();
        var privateSubrsOffset = FindPrivateSubrsOffsetFromPrivateDictionary(
            privateDictBytes.Buffer.Slice(0, privateSize));

        if (privateSubrsOffset == 0) return new CffIndex(source, 0, 0);

        await pipe.AdvanceToLocalPositionAsync(privateSubrsOffset+privateOffset).CA();
        return await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
    }

    // Per Adobe Techical Note 5176 page 24
    private const int subrsInstruction = 19;
    private long FindPrivateSubrsOffsetFromPrivateDictionary(ReadOnlySequence<byte> slice)
    {
        Span<DictValue> result = stackalloc DictValue[1];
        return new DictParser<CffDictionaryDefinition>(new SequenceReader<byte>(slice), result)
            .TryFindEntry(subrsInstruction)
            ? result[0].IntValue
            : 0;
    }

    //per Adobe Technical note 5716 page 15
    private const int charStringsInstruction = 17;
    private const int privateInstruction = 18;
    private static void ParseTopDict(ReadOnlySequence<byte> first, 
        out int charStringOffset, out int privateOffset, out int privateSize)
    {
        charStringOffset = privateOffset = privateSize = 0;
        Span<DictValue> result = stackalloc DictValue[2];
        var dictParser = new DictParser<CffDictionaryDefinition>(new SequenceReader<byte>(first), result);
        while (dictParser.ReadNextInstruction() is var instr and not 255)
        {
            switch (instr)
            {
                case charStringsInstruction:
                    charStringOffset = (int)result[0].IntValue;
                    break;
                case privateInstruction:
                    privateSize = (int)result[0].IntValue;
                    privateOffset = (int)result[1].IntValue;
                    break;
            }
        }
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