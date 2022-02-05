using System.IO;
using System.Numerics;
using System.Threading.Tasks;
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

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => new Type3Writer(this, target);

    private ValueTask<(double width, double height)> AddGlyphToCurrentString(byte b,
        Matrix3x2 charMatrix, IFontTarget target)
    {
        return target.RenderType3Character(
            characters[b - firstCharacter].CreateReader(), fontMatrix );
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

        public ValueTask<(double width, double height)> AddGlyphToCurrentString(
            byte b, Matrix3x2 textMatrix) => parent.AddGlyphToCurrentString(b, textMatrix, target);
        
        public void RenderCurrentString(bool stroke, bool fill, bool clip) { }    
    }
}