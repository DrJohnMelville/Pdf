using System.Buffers;
using System.Net.Http.Headers;
using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal partial class CffGenericFont : ListOf1GenericFont, ICMapSource, IGlyphWidthSource
{
    [FromConstructor] private readonly IMultiplexSource source;
    [FromConstructor] private readonly ushort unitsPerEm;
    [FromConstructor] public string Name { get; }
    [FromConstructor] private readonly long stringIndexOffset;
    [FromConstructor] private readonly long charStringOffset;
    [FromConstructor] private readonly long privateOffset;
    [FromConstructor] private readonly long privateSize;
    [FromConstructor] private readonly GlyphSubroutineExecutor globalSubroutineExecutor;
    [FromConstructor] private readonly long charSetOffset;
    [FromConstructor] private readonly long encodingOffset;

    public override ValueTask<ICMapSource> GetCmapSourceAsync() => new(this);

    public override async ValueTask<IGlyphSource> GetGlyphSourceAsync() =>
        new CffGlyphSource(await ReadCharStringIndexAsync(charStringOffset).CA(),
            globalSubroutineExecutor,
            new GlyphSubroutineExecutor(await GetPrivateSubrsAsync(privateOffset, privateSize).CA()),
            Matrix3x2.CreateScale(1f / unitsPerEm), []);

    private async ValueTask<CffIndex> ReadCharStringIndexAsync(long charStringOffset)
    {
        using var pipe = source.ReadPipeFrom(charStringOffset, charStringOffset);
        var charStringsIndex = await new CFFIndexParser(source, pipe).ParseCff1Async().CA();
        return charStringsIndex;
    }

    private async ValueTask<CffIndex> GetPrivateSubrsAsync(long privateOffset, long privateSize)
    {
        using var pipe = source.ReadPipeFrom(privateOffset, privateOffset);
        var privateDictBytes = await pipe.ReadAtLeastAsync((int)privateSize).CA();
        var privateSubrsOffset = FindPrivateSubrsOffsetFromPrivateDictionary(
            privateDictBytes.Buffer.Slice(0, privateSize));

        if (privateSubrsOffset == 0) return new CffIndex(source, 0, 0);

        var pipe2Position = privateSubrsOffset + privateOffset;
        using var pipe2 = source.ReadPipeFrom(pipe2Position, pipe2Position);
        return await new CFFIndexParser(source, pipe2).ParseCff1Async().CA();
    }

    // Per Adobe Technical Note 5176 page 24
    private const int subrsInstruction = 19;

    private long FindPrivateSubrsOffsetFromPrivateDictionary(ReadOnlySequence<byte> slice)
    {
        Span<DictValue> result = stackalloc DictValue[1];
        return new DictParser<CffDictionaryDefinition>(new SequenceReader<byte>(slice), result)
            .TryFindEntry(subrsInstruction)
            ? result[0].IntValue
            : 0;
    }

    public override async ValueTask<string[]> GlyphNamesAsync()
    {
        if (charSetOffset < 3)
            return new StandardCharsetFactory().FromByte(charSetOffset);

        var charStringindex = await ReadCharStringIndexAsync(charStringOffset).CA();
        using var stringsPipe =
            source.ReadPipeFrom(stringIndexOffset, stringIndexOffset);
        var strings = new CffStringIndex(await new CFFIndexParser(source, stringsPipe)
            .ParseCff1Async().CA());
        using var charsetPipe =
            source.ReadPipeFrom(charSetOffset, charSetOffset);
        return await new CharSetReader(strings, charsetPipe, charStringindex.Length
        ).ReadCharSetAsync().CA();
    }

    public override ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync() => new(this);
    public override ValueTask<string> FontFamilyNameAsync() => new(Name);
    public override ValueTask<MacStyles> GetFontStyleAsync() => new(MacStyles.None);

    int ICMapSource.Count => 1;

    public async ValueTask<ICmapImplementation> GetByIndexAsync(int index) =>
        new SingleArrayCmap<byte>(1, 0,
            encodingOffset switch
            {
#warning check for standard encodings as per adobe technical note 5176 page 20
                _ => await new CffEncodingReader(source.ReadPipeFrom(encodingOffset)).ParseAsync().CA()
            });

    public ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding) =>
        new((ICmapImplementation?)null);

    public (int platform, int encoding) GetPlatformEncoding(int index) => (4, 0);
    public float GlyphWidth(ushort glyph) => 0f;
}