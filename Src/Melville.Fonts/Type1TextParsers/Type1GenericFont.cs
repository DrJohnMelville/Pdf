using System.Buffers;
using System.Numerics;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;
using Melville.Postscript.Interpreter.Values;
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
    public IPostscriptDictionary? Dictionary { get; }
    private readonly string[] glyphNames;
    private readonly Memory<byte>[] charStrings;
    private readonly int notDefIndex;
    private readonly Matrix3x2 glyphTransform;
    private readonly IGlyphSubroutineExecutor subroutines;
    private readonly IPostscriptArray otherSubrs;

    internal Type1GenericFont(
        IPostscriptDictionary? dictionary, 
        string[] glyphNames, 
        Memory<byte>[] charStrings, 
        int notDefIndex, 
        Matrix3x2 glyphTransform, 
        IGlyphSubroutineExecutor subroutines, IPostscriptArray otherSubrs)
    {
        Dictionary = dictionary;
        this.glyphNames = glyphNames;
        this.charStrings = charStrings;
        this.notDefIndex = notDefIndex;
        this.glyphTransform = glyphTransform;
        this.subroutines = subroutines;
        this.otherSubrs = otherSubrs;
    }

    /// <inheritdoc />
    public override ValueTask<ICMapSource> GetCmapSourceAsync() => new(this);

    /// <inheritdoc />
    public override ValueTask<IGlyphSource> GetGlyphSourceAsync() => new(this);

    /// <inheritdoc />
    public override ValueTask<string[]> GlyphNamesAsync() => new(glyphNames);

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
    public int GlyphCount => glyphNames.Length;

    /// <inheritdoc />
    public ValueTask RenderGlyphAsync<T>(uint glyph, T target, Matrix3x2 transform) where T : IGlyphTarget => 
        RenderToCffGlyphTarget(glyph, new CffGlyphTargetWrapper<T>(target), transform);

    /// <summary>
    /// Render a chisen glyph to an ICffGlyphTarget
    /// </summary>
    /// <param name="glyph">Index of the glypg to render</param>
    /// <param name="targetWrapper">The target to render the glyph on.</param>
    /// <param name="transform">A transform matrix to apply to all the point in the glyph</param>
    /// <typeparam name="T">The concrete type of the ICffGlyphVTarget</typeparam>
    public async ValueTask RenderToCffGlyphTarget<T>(
        uint glyph, T targetWrapper, Matrix3x2 transform) where T : ICffGlyphTarget
    {
        if (glyph >= charStrings.Length) glyph = (uint)notDefIndex;
        var executor = ObjectPool<CffInstructionExecutor<T>>.Shared.Rent().With(
            targetWrapper, glyphTransform*transform,
            subroutines, subroutines, []);
        await executor.ExecuteInstructionSequenceAsync(
            new ReadOnlySequence<byte>(charStrings[glyph])).CA();
        ObjectPool<CffInstructionExecutor<T>>.Shared.Return(executor);
    }

    /// <inheritdoc />
    public float GlyphWidth(ushort glyph) => 0;
}