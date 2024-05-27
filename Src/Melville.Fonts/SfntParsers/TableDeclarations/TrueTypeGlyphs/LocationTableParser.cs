using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// Get the location of a glyph definition within the glyph stream.
/// </summary>
public interface IGlyphLocationSource
{
    /// <summary>
    /// Get the stream location of the desired glyph;
    /// </summary>
    /// <param name="glyph">The desired glyph index</param>
    /// <returns>Ther offset and length for the glyph outline data.</returns>
    GlyphLocation GetLocation(uint glyph);

    /// <summary>
    /// Number of glyphs in the table
    /// </summary>
    int TotalGlyphs { get; }
}


internal readonly struct LocationTableParser(PipeReader source, ushort numGlyphs, short style)
{
    public ValueTask<IGlyphLocationSource> ParseAsync()
    {
        return style == 0 ? ParseShortStyleAsync(): ParseLongStyleAsync();
    }

    private async ValueTask<IGlyphLocationSource> ParseShortStyleAsync()
    {
        var values = new ushort[numGlyphs+1];
        await FieldParser.ReadAsync(source, values).CA();
        return new GlyphLocationSourceType0(values);
    }

    private async ValueTask<IGlyphLocationSource> ParseLongStyleAsync()
    {
        var values = new uint[numGlyphs+1];
        await FieldParser.ReadAsync(source, values).CA();
        return new GlyphLocationSourceType1(values);
    }
}

internal abstract class GlyphLocationSource<T>(T[] values) : IGlyphLocationSource
{
    public GlyphLocation GetLocation(uint glyph)
    {
        if (glyph +1 >= values.Length) return new GlyphLocation(0, 0);
        uint value = OffsetValue(values[glyph]);
        uint nextValue = OffsetValue(values[glyph+1]);
        return new GlyphLocation(value, nextValue - value);
    }

    protected abstract uint OffsetValue(T value);

    public int TotalGlyphs => values.Length - 1;
}

[FromConstructor]
internal partial class GlyphLocationSourceType0: GlyphLocationSource<ushort>
{
    protected override uint OffsetValue(ushort glyph) => ((uint)glyph)<<1;
}

[FromConstructor]
internal partial class GlyphLocationSourceType1: GlyphLocationSource<uint>
{
    protected override uint OffsetValue(uint glyph) => glyph;
}

/// <summary>
/// This is the location of a glyph program within the glyph stream;
/// </summary>
public readonly partial struct GlyphLocation
{
    /// <summary>
    /// Index of the glyph program
    /// </summary>
    [FromConstructor] public uint Offset { get; }

    /// <summary>
    /// Length of the glyph program.
    /// </summary>
    [FromConstructor] public uint Length { get; }
}