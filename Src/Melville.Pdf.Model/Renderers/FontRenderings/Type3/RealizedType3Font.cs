using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

internal partial class RealizedType3Font : IRealizedFont, IMapCharacterToGlyph, IDisposable
{
    [FromConstructor]private readonly IMultiplexSource[] characters;
    [FromConstructor]private readonly byte firstCharacter;
    [FromConstructor]private readonly Matrix3x2 fontMatrix;
    [FromConstructor]private readonly PdfDictionary rawFont;

    public IReadCharacter ReadCharacter => SingleByteCharacters.Instance;
    public IMapCharacterToGlyph MapCharacterToGlyph => this;

    public uint GetGlyph(uint character) => 
        (uint)Math.Clamp(character - firstCharacter, 0, characters.Length-1);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => new Type3Writer(this, target);
    
    private async ValueTask<double> AddGlyphToCurrentStringAsync(uint glyph,
        Matrix3x2 charMatrix, IFontTarget target)
    {
        await using var readFrom = characters[glyph].ReadFrom(0);
        return await target.RenderType3CharacterAsync(
            readFrom, fontMatrix, rawFont).CA();
    }
    public double? CharacterWidth(uint character) => default;

    public int GlyphCount => characters.Length;
    public string FamilyName => "Type 3 Font";
    public string Description => "";
    public bool IsCachableFont => false;

    private class Type3Writer: IFontWriteOperation
    {
        private readonly RealizedType3Font parent;
        private readonly IFontTarget target;
        private double cachedGlyphWidth;

        public Type3Writer(RealizedType3Font parent, IFontTarget target)
        {
            this.parent = parent;
            this.target = target;
        }

        public async ValueTask AddGlyphToCurrentStringAsync(uint character, uint glyph, Matrix3x2 textMatrix) => 
            cachedGlyphWidth = await parent.AddGlyphToCurrentStringAsync(glyph, textMatrix, target).CA();

        public ValueTask<double> NativeWidthOfLastGlyphAsync(uint glyph) => new(cachedGlyphWidth);

        public void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 textMatrix)
        { }

        public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) => new Type3Writer(parent, target);
    }

    public void Dispose()
    {
        foreach (var ms in characters)
        {
            (ms as IDisposable)?.Dispose();
        }
    }
}