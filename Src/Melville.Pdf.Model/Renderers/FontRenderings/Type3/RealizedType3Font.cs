using System.IO;
using System.Numerics;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

public interface IType3FontTarget
{
    ValueTask<(double width, double height)> RenderType3Character(Stream s);
}

public class RealizedType3Font : IRealizedFont
{
    private readonly IType3FontTarget target;
    private readonly MultiBufferStream[] characters;
    private readonly byte firstCharacter;
    private readonly Matrix3x2 fontMatrix;
    
    public RealizedType3Font(
        IType3FontTarget target, MultiBufferStream[] characters, byte firstCharacter, 
        Matrix3x2 fontMatrix)
    {
        this.target = target;
        this.characters = characters;
        this.firstCharacter = firstCharacter;
        this.fontMatrix = fontMatrix;
    }

    public IFontWriteOperation BeginFontWrite()
    {
        throw new System.NotImplementedException();
    }
    
    private class FontWriteOperation: IFontWriteOperation
    {
        private readonly RealizedType3Font parent;

        public FontWriteOperation(RealizedType3Font parent)
        {
            this.parent = parent;
        }

        public ValueTask<(double width, double height)> AddGlyphToCurrentString(byte b)
        {
            return parent.target.RenderType3Character(
                parent.characters[b - parent.firstCharacter].CreateReader());
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
        }
    }
}