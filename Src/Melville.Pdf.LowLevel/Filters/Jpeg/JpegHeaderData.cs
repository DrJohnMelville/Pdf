using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegHeaderData: IImageSizeStream
{
    public int Width { get; }
    public int Height { get; }
    public int BitsPerComponent { get; }
    public ComponentData[] Components { get; }
    private readonly int[] commponentCoeffs;
    public int ImageComponents => Components.Length;

    public JpegHeaderData(int width, int height, int bitsPerComponent, ComponentData[] components)
    {
        Width = width;
        Height = height;
        BitsPerComponent = bitsPerComponent;
        Components = components;
        commponentCoeffs = new int[Components.Length];
    }

    public void UpdateHuffmanTables(ComponentId id, HuffmanTable dcTable, HuffmanTable acTable)
    {
        for (int i = 0; i < Components.Length; i++)
        {
            if (Components[i].Id == id)
            {
                Components[i] = Components[i] with { DcHuffman = dcTable, AcHuffman = acTable };
                return;
            }
        }
        throw new PdfParseException($"Cannot find JPEG component {id}");
    }
}

public readonly partial struct ComponentData
{
    [FromConstructor] public ComponentId Id { get; }
    [FromConstructor] private readonly int samplingFactors;
    [FromConstructor] public int QuantTableNumber { get; }

    public HuffmanTable AcHuffman { get; init; } = HuffmanTable.Empty;
    public HuffmanTable DcHuffman { get; init; } = HuffmanTable.Empty;

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