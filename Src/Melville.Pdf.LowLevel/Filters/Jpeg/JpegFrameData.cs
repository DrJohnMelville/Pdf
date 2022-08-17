using Melville.INPC;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegFrameData: IImageSizeStream
{
    [FromConstructor] public int Width { get; }
    [FromConstructor] public int Height { get; }
    [FromConstructor] public int BitsPerComponent { get; }
    [FromConstructor] public CompnentData[] Components { get; }
    public int ImageComponents => Components.Length;
}

public readonly partial struct CompnentData
{
    [FromConstructor] public ComponentId Id { get; }
    [FromConstructor] private readonly int samplingFactors;
    [FromConstructor] public int QuantTableNumber { get; }

    public int HorizontalSamplingFactor => samplingFactors & 0b1111;
    public int VerticalSamplingFactor => (samplingFactors>>4) & 0b1111;
    
}

public enum ComponentId
{
    Y = 1,
    Cb = 2,
    Cr = 3,
    I = 4,
    Q = 5
}