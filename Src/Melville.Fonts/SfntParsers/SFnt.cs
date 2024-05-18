using System.Runtime.CompilerServices;
using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.INPC;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers;

/// <summary>
/// Represents a font that uses the Sfnt table structure to represent its tables
/// </summary>
public partial class SFnt : ListOf1GenericFont, IDisposable
{
   public void Dispose() => source.Dispose();
   [FromConstructor] private IMultiplexSource source;
   /// <summary>
   /// These are the tables that make up the font.
   /// </summary>
   [FromConstructor] public IReadOnlyList<TableRecord> Tables { get; }
}