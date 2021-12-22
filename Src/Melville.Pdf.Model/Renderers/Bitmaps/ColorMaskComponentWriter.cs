using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class ColorMaskComponentWriter: IComponentWriter
{
    private readonly IComponentWriter innerWriter;
    private readonly (int,int)[] HiddenInterval;
    private readonly int[] partials;
    private int current = 0;

    public ColorMaskComponentWriter(IComponentWriter innerWriter, int[] values)
    {
        this.innerWriter = innerWriter;
        if (values.Length % 2 == 1) throw new PdfParseException("ColorMask with odd number of elements");
        HiddenInterval = new (int,int)[values.Length / 2];
        for (int i = 0; i < HiddenInterval.Length; i++)
        {
            HiddenInterval[i] = (values[2 * i], values[2 * i + 1]);
        }

        partials = new int[HiddenInterval.Length];
    }

    public unsafe void WriteComponent(ref byte* target, int component)
    {
        partials[current++] = component;
        if (current >= partials.Length)
            PushValue(ref target);
    }

    private unsafe void PushValue(ref byte* target)
    {
        current = 0;
        if (IsInvisible())
            BitmapPointerMath.PushPixel(ref target, DeviceColor.Invisible);
        else
            PushCurrentPixel(ref target);
    }

    private bool IsInvisible()
    {
        for (int i = 0; i < partials.Length; i++)
        {
            if (OutsideSingleInterval(i, partials[i], HiddenInterval[i]))
                return false;
        }
        return true;
    }

    private bool OutsideSingleInterval(int i, int value, (int, int) interval) => value < interval.Item1 || value > interval.Item2;

    private unsafe void PushCurrentPixel(ref byte* target)
    {
        foreach (var component in partials)
        {
            innerWriter.WriteComponent(ref target, component);
        }
    }
}