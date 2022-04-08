using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class ColorMaskComponentWriter: IComponentWriter
{
    private readonly IComponentWriter innerWriter;
    private readonly (int,int)[] HiddenInterval;
  
    public ColorMaskComponentWriter(IComponentWriter innerWriter, int[] values)
    {
        this.innerWriter = innerWriter;
        if (values.Length % 2 == 1) 
            throw new PdfParseException("ColorMask with odd number of elements");
        HiddenInterval = new (int,int)[values.Length / 2];
        for (int i = 0; i < HiddenInterval.Length; i++)
        {
            HiddenInterval[i] = (values[2 * i], values[2 * i + 1]);
        }

    }

    public int ColorComponentCount => innerWriter.ColorComponentCount;
    
    public unsafe void WriteComponent(ref byte* target, int[] component, byte alpha)
    {
        innerWriter.WriteComponent(ref target, component, (byte)(IsInvisible(component) ? 0 : 255));
    }

    private bool IsInvisible(int[] component)
    {
        for (int i = 0; i < component.Length; i++)
        {
            if (OutsideSingleInterval(i, component[i], HiddenInterval[i]))
                return false;
        }
        return true;
    }

    private bool OutsideSingleInterval(int i, int value, (int, int) interval) => value < interval.Item1 || value > interval.Item2;
}