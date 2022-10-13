﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

[StaticSingleton]
public sealed partial class NullRealizedFont: IFontWriteOperation, IRealizedFont
{
    public (uint character, uint glyph, int bytesConsumed) GetNextGlyph(in ReadOnlySpan<byte> input) => (0, 0, 1);

    public ValueTask<double> AddGlyphToCurrentString(uint glyph, Matrix3x2 textMatrix) => new(0.0);

    public double CharacterWidth(uint character, double defaultWidth) => defaultWidth;

    public void RenderCurrentString(bool stroke, bool fill, bool clip)
    {
    }

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => this;
    public IFontWriteOperation BeginFontWriteWithoutTakingMutex(IFontTarget target) => this;

    public void Dispose() { }
}