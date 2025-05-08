using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal class TopDictData: IDisposable
{
    public long CharStringOffset { get; private set; }
    public long PrivateOffset { get; private set; }
    public long PrivateSize { get; private set; }
    public long CharsetOffset { get; private set; }
    public long EncodingOffset { get; private set; }
    public long StringIndexOffset { get; private set; }
    public long FDArrayOffset { get; private set; }
    public long FDSelectOffset { get; private set; }

    //per Adobe Technical note 5716 page 15
    private const int charSetInstruction = 15;
    private const int encodingInstruction = 16;
    private const int charStringsInstruction = 17;
    private const int privateInstruction = 18;
    private const int fdArrayInstruction = 0xC24;
    private const int fdSelectInstruction = 0xC25;

    private readonly IMultiplexSource source;


    public TopDictData(IMultiplexSource source, long charStringOffset, long privateOffset, long privateSize, long charsetOffset, long encodingOffset, long stringIndexOffset)
    {
        this.source = source;
        CharStringOffset = charStringOffset;
        PrivateOffset = privateOffset;
        PrivateSize = privateSize;
        CharsetOffset = charsetOffset;
        EncodingOffset = encodingOffset;
        StringIndexOffset = stringIndexOffset;
    }

    public TopDictData(IMultiplexSource source, long stringIndexOffset, DisposableSequence sourceData, TopDictData? prior)
    {

        this.source = source;
        StringIndexOffset = stringIndexOffset;

        TryDuplicateFromPrior(prior);

        ParseValuesFromSequence(sourceData);
    }

    private void TryDuplicateFromPrior(TopDictData? prior)
    {
        if (prior is not null)
        {
            CharStringOffset = prior.CharStringOffset;
            PrivateOffset = prior.PrivateOffset;
            PrivateSize = prior.PrivateSize;
            CharsetOffset = prior.CharsetOffset;
            EncodingOffset = prior.EncodingOffset;
        }
    }

    private void ParseValuesFromSequence(DisposableSequence sourceData)
    {
        Span<DictValue> result = stackalloc DictValue[2];
        var dictParser = new DictParser<CffDictionaryDefinition>(
            new SequenceReader<byte>(sourceData.Sequence), sourceData.Bookmark, result);
        while (dictParser.ReadNextInstruction() is var instr and not 255)
        {
            CaptureDesiredValue(instr, result);
        }
    }

    private void CaptureDesiredValue(int instr, Span<DictValue> result)
    {
        switch (instr)
        {
            case encodingInstruction:
                EncodingOffset = result[0].IntValue;
                break;
            case charSetInstruction:
                CharsetOffset = result[0].IntValue;
                break;
            case charStringsInstruction:
                CharStringOffset = result[0].IntValue;
                break;
            case privateInstruction:
                PrivateSize = result[0].IntValue;
                PrivateOffset = result[1].IntValue;
                break;
            case fdArrayInstruction:
                FDArrayOffset = result[0].IntValue;
                break;
            case fdSelectInstruction:
                FDSelectOffset = result[0].IntValue;
                break;
        }
    }

    public void Dispose() => source.Dispose();

    public async ValueTask<CffIndex> ReadCharStringIndexAsync()
    {
        using var pipe = source.ReadPipeFrom(CharStringOffset, CharStringOffset);
        source.AddParseMapAlias(pipe);
        pipe.IndentParseMap("Character Strings");
        pipe.JumpToParseMap(0);
        var charStringsIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        pipe.OutdentParseMap();
        return charStringsIndex;
    }

    public async ValueTask<T> HandlePrivateDictBytesAsync<T>(Func<ReadOnlySequence<byte>,T> operation)
    {
        using var pipe = source.ReadPipeFrom(PrivateOffset, PrivateOffset);
        var privateDictBytes = await pipe.ReadAtLeastAsync((int)PrivateSize).CA();
        return operation(privateDictBytes.Buffer.Slice(0, (int)PrivateSize));
    }

    public async Task<CffIndex> GetFdArrayAsync()
    {
        using var pipe = source.ReadLoggedPipeFrom((int)FDArrayOffset, (int)FDArrayOffset);
        pipe.IndentParseMap("Index");
        pipe.JumpToParseMap(0);
        var index = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        pipe.OutdentParseMap();
        return index;
    }

    public async ValueTask<CffIndex> StringIndexAsync()
    {
        using var stringsPipe = source.ReadPipeFrom(StringIndexOffset, StringIndexOffset);
        return await  new CFFIndexParser(source, stringsPipe).ParseCff1Async().CA();
    }

    public IByteSource CharsetPipe() => source.ReadPipeFrom(CharsetOffset, CharsetOffset);

    public IByteSource EncodingPipe() => source.ReadPipeFrom(EncodingOffset, EncodingOffset);

    public async ValueTask<CffIndex> GetPrivateSubrsAsync()
    {
        var privateSubrsOffset =
            await HandlePrivateDictBytesAsync(FindPrivateSubrsOffsetFromPrivateDictionary).CA();

        if (privateSubrsOffset == 0) return EmptyIndex();

        var pipe2Position = privateSubrsOffset + PrivateOffset;
        using var pipe2 = source.ReadPipeFrom(pipe2Position, pipe2Position);
        return await new CFFIndexParser(source, pipe2).ParseCff1Async().CA();
    }

    // Per Adobe Technical Note 5176 page 24
    private const int subrsInstruction = 19;

    private long FindPrivateSubrsOffsetFromPrivateDictionary(ReadOnlySequence<byte> slice)
    {
        Span<DictValue> result = stackalloc DictValue[1];
        return new DictParser<CffDictionaryDefinition>(
                new SequenceReader<byte>(slice), null, result)
            .TryFindEntry(subrsInstruction)
            ? result[0].IntValue
            : 0;
    }


    public CffIndex EmptyIndex()=> new CffIndex(source, 0, 0, null);
    public bool IsLoggingParseMap() => source.IsLoggingParseMap();
    public ParseMapBookmark? BookmarkAt(long location) => source.CreateParseMapBookmark((int)location);

    public IByteSource FdSelectPipe() =>
        source.ReadLoggedPipeFrom((int)FDSelectOffset, (int)FDSelectOffset);
}