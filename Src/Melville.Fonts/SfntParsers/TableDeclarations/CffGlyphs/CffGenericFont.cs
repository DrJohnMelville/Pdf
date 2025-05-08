using System.Buffers;
using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ParserMapping;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal partial class CffGenericFont : 
    ListOf1GenericFont, ICMapSource, IGlyphWidthSource, IDisposable
{
    [FromConstructor] private readonly ushort unitsPerEm;
    [FromConstructor] public string Name { get; }
    [FromConstructor] protected readonly CffIndex charStringIndex;
    [FromConstructor] private readonly GlyphSubroutineExecutor globalSubroutineExecutor;
    [FromConstructor] private readonly TopDictData topDictData;
    /// <inheritdoc />
    [FromConstructor] public override CidToGlyphMappingStyle TypeGlyphMapping { get; }

    public override ValueTask<ICMapSource> GetCmapSourceAsync() => new(this);

    public override async ValueTask<IGlyphSource> GetGlyphSourceAsync() =>
        new CffGlyphSource(charStringIndex,
            globalSubroutineExecutor,
            new GlyphSubroutineExecutor(await GetPrivateSubrsAsync().CA()),
            Matrix3x2.CreateScale(1f / unitsPerEm), []);

    private ValueTask<CffIndex> GetPrivateSubrsAsync()
    {
        return topDictData.GetPrivateSubrsAsync();
    }


    public override async ValueTask<string[]> GlyphNamesAsync()
    {
        var strings = await ReadStringIndexAsync().CA();
        var target = new StringTarget(strings, charStringIndex.Length);

        return (await MapCharSetAsync(target).CA()).Result;
    }

    private async Task<CffStringIndex> ReadStringIndexAsync()
    {
        // this is not the specified behavior, which is that an empty string index
        // is represented as an empty index.  However special indexes are used for
        // standard encodings and names so this is a reasonable possibility somoene
        // would try this, and it makes testing easier.  An offset of 0 would be an
        // invalid file otherwise, so this is not going to misread any valid font
        // file.
        if (topDictData.StringIndexOffset == 0)
            return new CffStringIndex(topDictData.EmptyIndex());

        return new CffStringIndex(await topDictData.StringIndexAsync().CA());
    }

    protected async ValueTask<T> MapCharSetAsync<T>(T target) where T: ICharSetTarget
    {
        if (topDictData.CharsetOffset< 3)
            return await new StandardCharsetFactory<T>(target)
                .FromByteAsync(topDictData.CharsetOffset).CA();
        
        using var charsetPipe = topDictData.CharsetPipe();
        return await new CharSetReader<T>(charsetPipe, target).ReadCharSetAsync().CA();
    }

    public override ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync() => new(this);
    public override ValueTask<string> FontFamilyNameAsync() => new(Name);
    public override ValueTask<MacStyles> GetFontStyleAsync() => new(MacStyles.None);

    int ICMapSource.Count => 1;

    public async ValueTask<ICmapImplementation> GetByIndexAsync(int index)
    {
        return await ReadCmapAsync(null).CA();
    }

    protected virtual async Task<ICmapImplementation> ReadCmapAsync(ParseMapBookmark? parseMapAncestor)
    {
        var data = ArrayPool<ushort>.Shared.Rent((int)charStringIndex.Length);
        var target = new MemoryTarget(data.AsMemory(0,(int)charStringIndex.Length));
        await MapCharSetAsync(target).CA();

        using var sidDecoder = new GlyphFromSid(data);

        return new SingleArrayCmap<byte>(1, 0,
            topDictData.EncodingOffset switch
            {
                0 => new PredefinedEncodings(sidDecoder).Standard(),
                1 => new PredefinedEncodings(sidDecoder).Expert(),
                _ => await ReadCustomEncoding(sidDecoder, parseMapAncestor)
            });
    }

    private ConfiguredValueTaskAwaitable<byte[]> ReadCustomEncoding(GlyphFromSid sidDecoder,
        ParseMapBookmark? mapAncestor)
    {
        var readPipeFrom = topDictData.EncodingPipe();
        mapAncestor.AddParseMapAlias(readPipeFrom);
        return new CffEncodingReader(
            readPipeFrom, sidDecoder).ParseAsync().CA();
    }

    public ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding) =>
        new((ICmapImplementation?)null);

    public (int platform, int encoding) GetPlatformEncoding(int index) => (4, 0);
    public float GlyphWidth(ushort glyph) => 0f;

    public void Dispose() => topDictData.Dispose();
}