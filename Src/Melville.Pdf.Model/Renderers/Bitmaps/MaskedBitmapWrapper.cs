using System;
using System.Threading.Tasks;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class MaskedBitmapWriter<TMaskType> : IComponentWriter where TMaskType: IMaskType, new()
{
    private readonly IComponentWriter innerWriter;
    protected MaskBitmap Mask { get; }
    private readonly double widthDelta;
    private readonly double heightDelta;
    private readonly int parentWidth;
    private readonly int parentHeight;
    private double row;
    private double col;
    private readonly TMaskType maskEvaluator = new();


    public MaskedBitmapWriter(IComponentWriter innerWriter, MaskBitmap mask,
        int parentWidth, int parentHeight)
    {
        this.innerWriter = innerWriter;
        Mask = mask;
        widthDelta = (double)mask.Width / (parentWidth);
        heightDelta = (double)mask.Height / (parentHeight);
        this.parentHeight = parentHeight;
        this.parentWidth = parentWidth;
        col = 0;
        row = parentHeight -1;
    }

    public int ColorComponentCount => innerWriter.ColorComponentCount;
    
    public unsafe void WriteComponent(ref byte* target, int[] component, byte alpha)
    {
        innerWriter.WriteComponent(ref target, component,AlphaForByte(alpha));
        FindNextPixel();
    }

    private byte AlphaForByte(byte alpha) =>
        maskEvaluator.AlphaForByte(alpha, Mask.PixelAt(
            (int)(row*heightDelta), (int)(col*widthDelta)));

    private void FindNextPixel()
    {
        col++;
        if (col < parentWidth) return;
        NextRow();
    }

    private void NextRow()
    {
        row --;
        col = 0;
    }
}