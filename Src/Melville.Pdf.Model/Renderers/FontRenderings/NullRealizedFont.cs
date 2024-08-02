using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

[StaticSingleton]
internal sealed partial class NullRealizedFont: IFontWriteOperation, IRealizedFont
{
    public IReadCharacter ReadCharacter => SingleByteCharacters.Instance;
    public IMapCharacterToGlyph MapCharacterToGlyph => IdentityCharacterToGlyph.Instance;

    public ValueTask AddGlyphToCurrentStringAsync(
        uint character, uint glyph, Matrix3x2 textMatrix) => default;

    public ValueTask<double> NativeWidthOfLastGlyphAsync(uint glyph) => new(0.0);
    
    public double? CharacterWidth(uint character) => default;

    public void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 textMatrix)
    {
    }

    public int GlyphCount => 0;
    public string FamilyName => "Null Font";
    public string Description => "Sentinel class that does not render anything";

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => this;
    public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) => this;
    public bool IsCachableFont => false;
}