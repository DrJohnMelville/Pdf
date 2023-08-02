using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

/// <summary>
/// This is the target of font writing operation.
/// </summary>
public interface IFontTarget
{
    /// <summary>
    /// Render the type 3 font character represented by the given matrix and font dictionary
    /// </summary>
    /// <param name="s">A content stream representing the character.</param>
    /// <param name="fontMatrix">The font matrix</param>
    /// <param name="fontDictionary">The dictionary defining the font -- which may contain resources.</param>
    /// <returns>A valuetask containing the width of the rendered character.</returns>
    ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix, PdfDictionary fontDictionary);
    /// <summary>
    /// Create a IDrawTarget that the stroked character can be drawn to.
    /// </summary>
    IDrawTarget CreateDrawTarget();
}

internal partial class RealizedType3Font : IRealizedFont
{
    [FromConstructor]private readonly MultiBufferStream[] characters;
    [FromConstructor]private readonly byte firstCharacter;
    [FromConstructor]private readonly Matrix3x2 fontMatrix;
    [FromConstructor]private readonly PdfDictionary rawFont;

    public (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input)
    {
        var character = input[0];
        return (character, ClampToGlyphs(character), 1);
    }

    private ushort ClampToGlyphs(byte input) => 
        (ushort)(input - firstCharacter).Clamp(0, characters.Length-1);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => new Type3Writer(this, target);
    
    private ValueTask<double> AddGlyphToCurrentStringAsync(uint glyph,
        Matrix3x2 charMatrix, IFontTarget target)
    {
        return target.RenderType3CharacterAsync(
            characters[glyph].CreateReader(), fontMatrix, rawFont);
    }
    public double CharacterWidth(uint character, double defaultWidth) => defaultWidth;

    public int GlyphCount => characters.Length;
    public string FamilyName => "Type 3 Font";
    public string Description => "";
    public bool IsCachableFont => false;

    private class Type3Writer: IFontWriteOperation
    {
        private readonly RealizedType3Font parent;
        private readonly IFontTarget target;

        public Type3Writer(RealizedType3Font parent, IFontTarget target)
        {
            this.parent = parent;
            this.target = target;
        }

        public ValueTask<double> AddGlyphToCurrentStringAsync(uint character, uint glyph, Matrix3x2 textMatrix) => 
            parent.AddGlyphToCurrentStringAsync(glyph, textMatrix, target);
        
        public void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 textMatrix)
        { }

        public void Dispose() { }
        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) => new Type3Writer(parent, target);
    }
}