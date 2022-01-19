using System.IO;
using System.Numerics;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

public interface IFontTarget
{
    ValueTask<(double width, double height)> RenderType3Character(Stream s, Matrix3x2 fontMatrix);
    IDrawTarget CreateDrawTarget();
}

public class RealizedType3Font : IRealizedFont, IFontWriteOperation
{
    private readonly IFontTarget target;
    private readonly MultiBufferStream[] characters;
    private readonly byte firstCharacter;
    private readonly Matrix3x2 fontMatrix;
    
    public RealizedType3Font(
        IFontTarget target, MultiBufferStream[] characters, byte firstCharacter, 
        Matrix3x2 fontMatrix)
    {
        this.target = target;
        this.characters = characters;
        this.firstCharacter = firstCharacter;
        this.fontMatrix = fontMatrix;
    }

    public IFontWriteOperation BeginFontWrite() => this;

        public ValueTask<(double width, double height)> AddGlyphToCurrentString(byte b)
        {
            return target.RenderType3Character(
                characters[b - firstCharacter].CreateReader(), fontMatrix);
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
        }
}