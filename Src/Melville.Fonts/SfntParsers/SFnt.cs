using System.Runtime.CompilerServices;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
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
    /// <summary>
    /// A MultiplexSource to get font data from.
    /// </summary>
    [FromConstructor] private IMultiplexSource source;
    private readonly TableCache cache = new();
    /// <inheritdoc />
    public void Dispose() => source.Dispose();

    /// <summary>
    /// The TableRecords that describe locations of tables in the font data
    /// </summary>
    [FromConstructor] private readonly TableRecord[] tables;

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
       new(cache.GetTable(SFntTableName.CMap, async ()=>
       {
           return FindTable(SFntTableName.CMap) is { } table
               ? ((ICMapSource)new ParsedCmap(source.OffsetFrom(table.Offset),
                   (await FieldParser.ReadFromAsync<CmapTable>(source.ReadPipeFrom(table.Offset)).CA()).Tables))
               : new ParsedCmap(source, []);
       }));
    
   /// <summary>
   /// Get a parsed header table from the SFnt
   /// </summary>
   /// <returns>The header table from the font.</returns>
   public Task<ParsedHead> HeadTableAsync() =>
       LoadTableAsync<ParsedHead>(SFntTableName.Head);

   /// <summary>
   /// Get a parsed Horizontal Header Table from the SFnt
   /// </summary>
   /// <returns></returns>
   public Task<ParsedHorizontalHeader> HorizontalHeaderTableAsync() =>
       LoadTableAsync<ParsedHorizontalHeader>(SFntTableName.HorizontalHeadder);

   /// <summary>
   /// Load the maximums table from the font
   /// </summary>
   public Task<ParsedMaximums> MaximumProfileTableAsync() =>
       cache.GetTable(SFntTableName.MaximumProfile, () => 
           FindTable(SFntTableName.MaximumProfile) is {} table?
               new MaxpParser(source.ReadPipeFrom(table.Offset)).ParseAsync().AsTask():
               Task.FromResult(new ParsedMaximums(0)));

   /// <summary>
   /// Get the horizontal metrics table from the font
   /// </summary>
   public Task<ParsedHorizontalMetrics> HorizontalMetricsAsync() =>
       DelayedLoadTableAsync(SFntTableName.HorizontalMetrics, CreateHorizontalMetricsAsync);

   private async Task<ParsedHorizontalMetrics> CreateHorizontalMetricsAsync()
   {
       if (FindTable(SFntTableName.HorizontalMetrics) is not { } table)
           return new ParsedHorizontalMetrics([], 0);

       var horizontalHeader = await HorizontalHeaderTableAsync().CA();
       var maximums = await MaximumProfileTableAsync().CA();
       return await new HorizontalMetricsParser(source.ReadPipeFrom(table.Offset),
           horizontalHeader.NumberOfHMetrics, maximums.NumGlyphs).ParseAsync().CA();
   }

   /// <summary>
   /// Parse the GlyphLocations table from the font
   /// </summary>
   /// <returns>An interface that can retrieve the offset and length of a glyph.</returns>
   public Task<IGlyphLocationSource?> GlyphLocationsAsync() =>
       DelayedLoadTableAsync(SFntTableName.GlyphLocations, CreateGlyphLocationsAsync);

   private async Task<IGlyphLocationSource?> CreateGlyphLocationsAsync()
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
        new(DelayedLoadTableAsync(SFntTableName.GlyphData, LoadGlyphSourceAsync));

   private Task<IGlyphSource> LoadGlyphSourceAsync()
   {
       if (FindTable(SFntTableName.GlyphData) is { } trueType)
            return LoadTrueTypeGlyphSourceAsync(trueType);
       if (FindTable(SFntTableName.CFF) is {} cff)
            return LoadCffGlyphSourceAsync(cff);
       throw new NotImplementedException("Cannot find Glyph Source");
   }

   private Task<IGlyphSource> LoadCffGlyphSourceAsync(TableRecord cff)
   {
       var parser = new CffGlyphSourceParser(source.OffsetFrom(cff.Offset));
       return parser.ParseAsync();
   }

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

   private async Task<T> DelayedLoadTableAsync<T>(uint tag, Func<Task<T>> method) =>
       await cache.GetTable(tag, ()=>
           new DelayTaskStart<T>(method));

   
   private Task<T> LoadTableAsync<T>(uint tag) where T : IGeneratedParsable<T>, new() =>
       cache.GetTable(tag, ()=>
           FindTable(tag) is {} table ? 
               FieldParser.ReadFromAsync<T>(source.ReadPipeFrom(table.Offset)).AsTask()
               : Task.FromResult(new T()));

   private TableRecord? FindTable(uint tag)
   {  
       var index = tables.AsSpan().BinarySearch(new TableRecord.Searcher(tag));
       return index < 0 ? null : tables[index];
   }

}

internal class DelayTaskStart<T>(Func<Task<T>> operation)
{
    private readonly Lazy<Task<T>> task = new(operation);

    public TaskAwaiter<T> GetAwaiter() => task.Value.GetAwaiter();
}