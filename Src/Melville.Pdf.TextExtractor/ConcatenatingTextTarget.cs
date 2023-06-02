using System.Numerics;
using System.Text;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.TextExtractor;

internal class ConcatenatingTextTarget: IExtractedTextTarget
{
    private readonly StringBuilder target = new StringBuilder();

    public void BeginWrite(IRealizedFont font)
    {
        if (target.Length > 0) target.AppendLine();
    }

    public void EndWrite(Matrix3x2 textMatrix)
    {
    }

    public void WriteCharacter(char character, Matrix3x2 textMatrix)
    {
        target.Append(character);
    }

    public string AllText() => target.ToString();
}