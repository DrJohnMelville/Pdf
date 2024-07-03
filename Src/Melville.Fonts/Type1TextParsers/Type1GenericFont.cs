using System.Numerics;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Fonts.Type1TextParsers;

/// <summary>
/// This class represents a Type 1 font in the type renderer
/// </summary>
public partial class Type1GenericFont: ListOf1GenericFont,
    IGenericFont, ICMapSource, IGlyphSource, IGlyphWidthSource
{
    /// <summary>
    /// The dictionary that was used tooo create the font.
    /// (this may always be null in release builds.)
    /// </summary>
    [FromConstructor] public IPostscriptDictionary? Dictionary { get; }

    /// <inheritdoc />
    public override ValueTask<ICMapSource> GetCmapSourceAsync() => new(this);

    /// <inheritdoc />
    public override ValueTask<IGlyphSource> GetGlyphSourceAsync() => new(this);

    /// <inheritdoc />
    public override ValueTask<string[]> GlyphNamesAsync() => new([]);

    /// <inheritdoc />
    public override ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync() => new(this);

    int ICMapSource.Count => 0;

    /// <inheritdoc />
    public ValueTask<ICmapImplementation> GetByIndexAsync(int index)
    {
        return new(new SingleArrayCmap<byte>(1,0,[]));
    }

    /// <inheritdoc />
    public ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding)
    {
        return new(new SingleArrayCmap<byte>(1,0,[]));
    }

    /// <inheritdoc />
    public (int platform, int encoding) GetPlatformEncoding(int index)
    {
        return (0,0);
    }

    /// <inheritdoc />
    public int GlyphCount => 0;

    /// <inheritdoc />
    public ValueTask RenderGlyphAsync<T>(uint glyph, T target, Matrix3x2 transform) where T : IGlyphTarget
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public float GlyphWidth(ushort glyph)
    {
        return 0;
    }
}