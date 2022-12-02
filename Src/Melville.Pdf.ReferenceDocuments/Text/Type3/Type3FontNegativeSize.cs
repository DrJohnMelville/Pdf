using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Text.Type3;

public class Type3FontNegativeSize : Type3FontBase
{
    public Type3FontNegativeSize() : base("Render a type 3 font with a negative size")
    {
        FontSize = -70;
    }

    protected override ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(300, 30));
        return base.DoPaintingAsync(csw);
    }
}