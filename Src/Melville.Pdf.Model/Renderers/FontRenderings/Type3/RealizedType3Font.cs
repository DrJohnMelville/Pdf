using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SpanAndMemory;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

public interface IFontTarget
{
    ValueTask<(double width, double height)> RenderType3Character(Stream s, Matrix3x2 fontMatrix);
    IDrawTarget CreateDrawTarget();
}

public class RealizedType3Font : IRealizedFont
{
    private readonly MultiBufferStream[] characters;
    private readonly byte firstCharacter;
    private readonly Matrix3x2 fontMatrix;
    
    public RealizedType3Font(MultiBufferStream[] characters, byte firstCharacter, 
        Matrix3x2 fontMatrix)
    {
        this.characters = characters;
        this.firstCharacter = firstCharacter;
        this.fontMatrix = fontMatrix;
    }

    public (uint glyph, int charsConsumed) GetNextGlyph(in ReadOnlySpan<byte> input) => 
        (ClampToGlyphs(input[0]), 1);

    private ushort ClampToGlyphs(byte input) => 
        (ushort)(input - firstCharacter).Clamp(0, characters.Length-1);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => new Type3Writer(this, target);

    private ValueTask<(double width, double height)> AddGlyphToCurrentString(uint glyph,
        Matrix3x2 charMatrix, IFontTarget target)
    {
        return target.RenderType3Character(
            characters[glyph].CreateReader(), fontMatrix);
    }

    private class Type3Writer: IFontWriteOperation
    {
        private readonly RealizedType3Font parent;
        private readonly IFontTarget target;

        public Type3Writer(RealizedType3Font parent, IFontTarget target)
        {
            this.parent = parent;
            this.target = target;
        }

        public async ValueTask<(double width, double height)> AddGlyphToCurrentString(
            uint glyph, Matrix3x2 textMatrix)
        {
            var (width, height) = await parent.AddGlyphToCurrentString(
                glyph, textMatrix, target).CA();
            return (width, height);
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip) { }    
    }
}