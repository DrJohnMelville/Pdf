using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

 partial class FontWithNegativeSize : IRealizedFont
{
    [DelegateTo] [FromConstructor] private readonly IRealizedFont font;

    public double CharacterWidth(uint character, double defaultWidth) =>
        -font.CharacterWidth(character, defaultWidth);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
        new InvertingFontWriter(font.BeginFontWrite(target));

    public IFontWriteOperation BeginFontWriteWithoutTakingMutex(IFontTarget target) => 
        new InvertingFontWriter(font.BeginFontWriteWithoutTakingMutex(target));
}

public partial class InvertingFontWriter : IFontWriteOperation
{
    [DelegateTo][FromConstructor] private readonly IFontWriteOperation inner;
    
    public ValueTask<double> AddGlyphToCurrentString(uint glyph, Matrix3x2 textMatrix) => 
        inner.AddGlyphToCurrentString(glyph, Matrix3x2.CreateScale(-1)* textMatrix);
}