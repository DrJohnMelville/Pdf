using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegHeaderData: IImageSizeStream
{
    [FromConstructor]public int Width { get; }
    [FromConstructor]public int Height { get; }
    [FromConstructor]public int BitsPerComponent { get; }
    [FromConstructor]public ComponentDefinition[] Components { get; }
    public int ImageComponents => Components.Length;
}

public enum ComponentId
{
    Y = 1,
    Cb = 2,
    Cr = 3,
    I = 4,
    Q = 5
}