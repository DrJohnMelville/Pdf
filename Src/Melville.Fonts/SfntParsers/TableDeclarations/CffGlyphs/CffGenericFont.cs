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
    [FromConstructor] private readonly CffIndex charStringIndex;
    [FromConstructor] private readonly long privateOffset;
    [FromConstructor] private readonly long privateSize;
    [FromConstructor] private readonly GlyphSubroutineExecutor globalSubroutineExecutor;
    [FromConstructor] private readonly long charSetOffset;
    [FromConstructor] private readonly long encodingOffset;

    public override ValueTask<ICMapSource> GetCmapSourceAsync() => new(this);

    public override async ValueTask<IGlyphSource> GetGlyphSourceAsync() =>
        new CffGlyphSource(charStringIndex,
            globalSubroutineExecutor,
            new GlyphSubroutineExecutor(
                await GetPrivateSubrsAsync(privateOffset, privateSize).CA()),
            Matrix3x2.CreateScale(1f / unitsPerEm), []);

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
        var strings = await ReadStringIndexAsync().CA();
        var target = new StringTarget(strings, charStringIndex.Length);

        return (await MapCharSet(target).CA()).Result;
    }

    private async Task<CffStringIndex> ReadStringIndexAsync()
    {
        // this is not the specified behavior, which is that an empty string index
        // is represented as an empty index.  However special indexes are used for
        // standard encodings and names so this is a reasonable possibility somoene
        // would try this, and it makes testing easier.  An offset of 0 would be an
        // invalid file otherwise, so this is not going to misread any valid font
        // file.
        if (stringIndexOffset == 0)
            return new CffStringIndex(new CffIndex(source, 0, 0));

        using var stringsPipe =
            source.ReadPipeFrom(stringIndexOffset, stringIndexOffset);
        var strings = new CffStringIndex(await new CFFIndexParser(source, stringsPipe)
            .ParseCff1Async().CA());
        return strings;
    }

    private ValueTask<T> MapCharSet<T>(T target) where T: ICharSetTarget
    {
        if (charSetOffset < 3)
            return new StandardCharsetFactory<T>(target).FromByte(charSetOffset);
        
        using var charsetPipe = source.ReadPipeFrom(charSetOffset, charSetOffset);
        return new CharSetReader<T>(charsetPipe, target).ReadCharSetAsync();
    }

    public override ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync() => new(this);
    public override ValueTask<string> FontFamilyNameAsync() => new(Name);
    public override ValueTask<MacStyles> GetFontStyleAsync() => new(MacStyles.None);

    int ICMapSource.Count => 1;

    public async ValueTask<ICmapImplementation> GetByIndexAsync(int index)
    {
        var data = ArrayPool<ushort>.Shared.Rent((int)charStringIndex.Length);
        var target = new MemoryTarget(data);
        await MapCharSet(target).CA();

        var sidDecoder = new GlyphFromSid(data);

        return new SingleArrayCmap<byte>(1, 0,
            encodingOffset switch
            {
                0 => new PredefinedEncodings(sidDecoder).Standard(),
                1 => new PredefinedEncodings(sidDecoder).Expert(),
                _ => await new CffEncodingReader(
                    source.ReadPipeFrom(encodingOffset)).ParseAsync().CA()
            });
    }

    public ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding) =>
        new((ICmapImplementation?)null);

    public (int platform, int encoding) GetPlatformEncoding(int index) => (4, 0);
    public float GlyphWidth(ushort glyph) => 0f;
}