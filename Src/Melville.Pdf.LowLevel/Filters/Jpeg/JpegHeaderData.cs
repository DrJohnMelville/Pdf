using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegHeaderData: IImageSizeStream
{
    public int Width { get; }
    public int Height { get; }
    public int BitsPerComponent { get; }
    public ComponentData[] Components { get; }
    public int ImageComponents => Components.Length;

    public JpegHeaderData(int width, int height, int bitsPerComponent, ComponentData[] components)
    {
        Width = width;
        Height = height;
        BitsPerComponent = bitsPerComponent;
        Components = components;
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

public enum ComponentId
{
    Y = 1,
    Cb = 2,
    Cr = 3,
    I = 4,
    Q = 5
}