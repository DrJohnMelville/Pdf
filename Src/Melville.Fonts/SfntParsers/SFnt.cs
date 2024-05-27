using System.Runtime.CompilerServices;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
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
   public override Task<ICMapSource> ParseCMapsAsync() =>
       cache.GetTable(SFntTableName.CMap, ()=>
       FindTable(SFntTableName.CMap) is {} table ? 
           TableLoader.LoadCmapAsync(source.OffsetFrom(table.Offset)).AsTask()
           : Task.FromResult<ICMapSource>(new ParsedCmap(source, [])));
    
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
   /// Load the maximums table from the parser
   /// </summary>
   /// <returns></returns>
   public Task<ParsedMaximums> MaximumProfileTableAsync() =>
       cache.GetTable(SFntTableName.MaximumProfile, () => 
           FindTable(SFntTableName.MaximumProfile) is {} table?
               new MaxpParser(source.ReadPipeFrom(table.Offset)).ParseAsync().AsTask():
               Task.FromResult(new ParsedMaximums(0)));

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