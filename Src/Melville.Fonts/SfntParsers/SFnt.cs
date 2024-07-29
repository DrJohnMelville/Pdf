using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Fonts.SfntParsers.TableDeclarations.Names;
using Melville.Fonts.SfntParsers.TableDeclarations.PostscriptDatas;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Hacks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers;

/// <summary>
/// Represents a font that uses the Sfnt table structure to represent its tables
/// </summary>
public partial class SFnt : ListOf1GenericFont, IDisposable
{
    private IMultiplexSource source;

    private readonly TableRecord[] tables;
    private readonly Lazy<Task<IGlyphSource>> glyphSource;
    private readonly Lazy<Task<ICMapSource>> cmapSource;
    private readonly Lazy<Task<ParsedHead>> headSource;
    private readonly Lazy<Task<ParsedHorizontalHeader>> horizHeadSource;
    private readonly Lazy<Task<ParsedHorizontalMetrics>> horizMetricsSource;
    private readonly Lazy<Task<ParsedMaximums>> maxSource;
    private readonly Lazy<Task<IGlyphLocationSource?>> glyphLocationSource;
    private readonly Lazy<Task<PostscriptData>> postscriptDataSource;
    private readonly Lazy<Task<INameTableView>> namesSource;


    /// <summary> Create a SFnt
    /// </summary>
    /// <param name="source">A MultiplexSource to get font data from.</param>
    /// <param name="tables">The TableRecords that describe locations of tables in the font data</param>
    public SFnt(IMultiplexSource source, TableRecord[] tables)
    {
        this.source = source;
        this.tables = tables;
        glyphSource = new(LoadGlyphSourceAsync);
        cmapSource = new(LoadCmapAsync);
        headSource = new(LoadHeadAsync);
        horizHeadSource = new(LoadHorizHeaderAsync);
        horizMetricsSource = new(LoadHorizontalMetricsAsync);
        maxSource = new(LoadMaxProfileAsync);
        glyphLocationSource = new(LoadGlyphLocationsAsync);
        postscriptDataSource = new(LoadPostscriptDataAsync);
        namesSource = new(LoadNameTableAsync);
    }

    /// <inheritdoc />
    public void Dispose() => source.Dispose();

    /// <summary>
    /// These are the tables that make up the font.
    /// </summary>
    public IReadOnlyList<TableRecord> Tables => tables;

    /// <summary>
    /// Load a font table as an array of bytes.  This is used by the font viewer in the tools. 
    /// </summary>
    /// <param name="table">The TableRecord corresponding to the desired font.</param>
    /// <returns>A byte array containing the desired table</returns>
    public async Task<byte[]> GetTableBytesAsync(TableRecord table)
    {
        var ret = new byte[table.Length];
        await using var stream = source.ReadFrom(table.Offset);
        await ret.FillBufferAsync(0, ret.Length, stream).CA();
        return ret;
    }

    /// <inheritdoc />
    public override ValueTask<ICMapSource> GetCmapSourceAsync() =>
        new(cmapSource.Value);

    private Task<ICMapSource> LoadCmapAsync()
    {
        return FindTable(SFntTableName.CMap) is { } table
            ? LoadCmapSlowAsync(table)
            : Task.FromResult((ICMapSource)new ParsedCmap(source, []));
    }

    private async Task<ICMapSource> LoadCmapSlowAsync(TableRecord table)
    {
        using var pipe = source.ReadPipeFrom(table.Offset);
        return ((ICMapSource)new ParsedCmap(source.OffsetFrom(table.Offset),
            (await FieldParser.ReadFromAsync<CmapTable>(pipe).CA()).Tables));
    }

    /// <summary>
    /// Get a parsed header table from the SFnt
    /// </summary>
    /// <returns>The header table from the font.</returns>
    public Task<ParsedHead> HeadTableAsync() =>
        headSource.Value;

    private Task<ParsedHead> LoadHeadAsync() =>
        LoadTableAsync<ParsedHead>(SFntTableName.Head);


    /// <summary>
    /// Get a parsed Horizontal Header Table from the SFnt
    /// </summary>
    /// <returns></returns>
    public Task<ParsedHorizontalHeader> HorizontalHeaderTableAsync() =>
        horizHeadSource.Value;

    private Task<ParsedHorizontalHeader> LoadHorizHeaderAsync =>
        LoadTableAsync<ParsedHorizontalHeader>(SFntTableName.HorizontalHeadder);

    /// <summary>
    /// Load the maximums table from the font
    /// </summary>
    public Task<ParsedMaximums> MaximumProfileTableAsync() => maxSource.Value;

    private Task<ParsedMaximums> LoadMaxProfileAsync() =>
        FindTable(SFntTableName.MaximumProfile) is { } table
            ? new MaxpParser(source.ReadPipeFrom(table.Offset)).ParseAsync().AsTask()
            : Task.FromResult(new ParsedMaximums(0));

    /// <inheritdoc />
    public override async ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync() =>
        await HorizontalMetricsAsync().CA();

    /// <summary>
    /// Get the horizontal metrics table from the font
    /// </summary>
    public Task<ParsedHorizontalMetrics> HorizontalMetricsAsync() =>
        horizMetricsSource.Value;

    private async Task<ParsedHorizontalMetrics> LoadHorizontalMetricsAsync()
    {
        if (FindTable(SFntTableName.HorizontalMetrics) is not { } table)
            return new ParsedHorizontalMetrics([], 0, 1);

        var horizontalHeader = await HorizontalHeaderTableAsync().CA();
        var maximums = await MaximumProfileTableAsync().CA();
        var head = await HeadTableAsync().CA();
        return await new HorizontalMetricsParser(source.ReadPipeFrom(table.Offset),
            horizontalHeader.NumberOfHMetrics, maximums.NumGlyphs, head.UnitsPerEm).ParseAsync().CA();
    }

    /// <summary>
    /// Parse the GlyphLocations table from the font
    /// </summary>
    /// <returns>An interface that can retrieve the offset and length of a glyph.</returns>
    public Task<IGlyphLocationSource?> GlyphLocationsAsync() =>
        glyphLocationSource.Value;

    private async Task<IGlyphLocationSource?> LoadGlyphLocationsAsync()
    {
        var maximums = await MaximumProfileTableAsync().CA();
        var head = await HeadTableAsync().CA();
        return FindTable(SFntTableName.GlyphLocations) is { } table
            ? await new LocationTableParser(
                    source.ReadPipeFrom(table.Offset), maximums.NumGlyphs, head.IndexToLocFormat)
                .ParseAsync().CA()
            : null;
    }

    /// <inheritdoc />
    public override ValueTask<IGlyphSource> GetGlyphSourceAsync() =>
        new(glyphSource.Value);

    private Task<IGlyphSource> LoadGlyphSourceAsync()
    {
        if (FindTable(SFntTableName.GlyphData) is { } trueType)
            return LoadTrueTypeGlyphSourceAsync(trueType);
        if (FindTable(SFntTableName.CFF) is { } cff)
            return LoadCffGlyphSourceAsync(cff);
        if (FindTable(SFntTableName.CFF2) is { } cff2)
            return LoadCff2GlyphSourceAsync(cff2);
        throw new NotImplementedException("Cannot find Glyph Source");
    }

    private async Task<IGlyphSource> LoadCff2GlyphSourceAsync(TableRecord cff)
    {
        var head = await HeadTableAsync().CA();
        var parser = new Cff2GlyphSourceParser(source.OffsetFrom(cff.Offset));
        return await parser.ParseAsync().CA();
    }

    public ValueTask<IReadOnlyList<IGenericFont>> InnerGenericFontsAsync() =>
        FindTable(SFntTableName.CFF) is { } cff ? 
            LoadInnerCffFont(cff) : new([]);

    private async ValueTask<IReadOnlyList<IGenericFont>> LoadInnerCffFont(TableRecord cff)
    {
        var head = await HeadTableAsync().CA();
        var parser = new CffGlyphSourceParser(source.OffsetFrom(cff.Offset),
            head.UnitsPerEm);
        return await parser.ParseGenericFontAsync().CA();
    }

    private async Task<IGlyphSource> LoadCffGlyphSourceAsync(TableRecord cff) => 
        await (await LoadInnerCffFont(cff).CA())[0].GetGlyphSourceAsync().CA();

    private async Task<IGlyphSource> LoadTrueTypeGlyphSourceAsync(TableRecord table)
    {
        var loc = await GlyphLocationsAsync().CA();
        if (loc is null)
            throw new InvalidDataException("GlyphLoc table is require for truetype outlines");
        var head = await HeadTableAsync().CA();
        var hMetrics = await HorizontalMetricsAsync().CA();
        return new TrueTypeGlyphSource(
            loc, source.OffsetFrom(table.Offset), head.UnitsPerEm, hMetrics);
    }

    private Task<T> LoadTableAsync<T>(uint tag) where T : IGeneratedParsable<T>, new() =>
        FindTable(tag) is { } table
            ? LoadTableSlowAsync<T>(table)
            : Task.FromResult(new T());

    private async Task<T> LoadTableSlowAsync<T>(TableRecord table) where T : IGeneratedParsable<T>, new()
    {
        using var pipe = source.ReadPipeFrom(table.Offset);
        return await FieldParser.ReadFromAsync<T>(pipe).CA();
    }

    private TableRecord? FindTable(uint tag)
    {
        var index = tables.AsSpan().BinarySearch(new TableRecord.Searcher(tag));
        return index < 0 ? null : tables[index];
    }

    private Task<PostscriptData> GetPostscriptDataAsync() =>
        postscriptDataSource.Value;

    private Task<PostscriptData> LoadPostscriptDataAsync() =>
        FindTable(SFntTableName.PostscriptData) is not { } table
            ? Task.FromResult(new PostscriptData())
            : new PostscriptTableParser(source.ReadPipeFrom(table.Offset)).ParseAsync().AsTask();

    /// <inheritdoc />
    public override async ValueTask<string[]> GlyphNamesAsync() =>
        (await GetPostscriptDataAsync().CA()).GlyphNames;

    /// <summary>
    /// Parse the name table from the font
    /// </summary>
    /// <returns>An object representing the parsed name table</returns>
    public Task<INameTableView> GetNamesAsync() => namesSource.Value;
    private async Task<INameTableView> LoadNameTableAsync()
    {
        return FindTable(SFntTableName.Name) is {} table
            ? await new NameTableParser(source.OffsetFrom(table.Offset))
                .ParseAsync().CA()
            : new NullNameTableView();
    }

    /// <inheritdoc />
    public override async ValueTask<string> FontFamilyNameAsync() => 
        await (await GetNamesAsync().CA()).GetNameAsync(SfntNameKey.FontFamilyName).CA()
        ??"";

    /// <inheritdoc />
    public override async ValueTask<MacStyles> GetFontStyleAsync() =>
        (await HeadTableAsync().CA()).MacStyle;
}