using System.Runtime.CompilerServices;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
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
    [FromConstructor] private IMultiplexSource source;

    /// <inheritdoc />
    public void Dispose() => source.Dispose();

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
   public override ValueTask<ICMapSource> ParseCMapsAsync() =>
       FindTable(SFntTableName.CMap) is {} table ? 
           TableLoader.LoadCmap(source.OffsetFrom(table.Offset))
           : new(new ParsedCmap(source, []));
    
   /// <summary>
   /// Get a parsed header table from the SFnt
   /// </summary>
   /// <returns>The header table from the font.</returns>
   public ValueTask<ParsedHead> HeadTableAsync() =>
       LoadTableAsync<ParsedHead>(SFntTableName.Head);

   public async ValueTask<ParsedHorizontalHeader> HorizontalHeaderTableAsync() =>
       await LoadTableAsync<ParsedHorizontalHeader>(SFntTableName.HorizontalHeadder);

   private ValueTask<T> LoadTableAsync<T>(uint tag) where T : IGeneratedParsable<T>, new() =>
       FindTable(tag) is {} table ? 
           FieldParser.ReadFromAsync<T>(source.ReadPipeFrom(table.Offset))
           : new(null);

   private TableRecord? FindTable(uint tag)
   {
       var index = tables.AsSpan().BinarySearch(new TableRecord.Searcher(tag));
       return index < 0 ? null : tables[index];
   }

}