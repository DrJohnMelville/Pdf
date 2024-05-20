using System.Runtime.CompilerServices;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Hacks;
using Melville.INPC;
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
   
    /// <summary>
   /// These are the tables that make up the font.
   /// </summary>
   [FromConstructor] public IReadOnlyList<TableRecord> Tables { get; }

   /// <summary>
   /// Load a font table as an array of bytes.  This is used by the font viewer in the tools. 
   /// </summary>
   /// <param name="table">The TableRecord corresponding to the desired font.</param>
   /// <returns>A byte array containing the desired table</returns>
   public async Task<byte[]> GetTableBytesAsync(TableRecord table)
   {
       var ret = new byte[table.Length];
       await using var stream = source.ReadFrom(table.Offset);
       await ret.FillBufferAsync(0, ret.Length, stream);
       return ret;
   }
}