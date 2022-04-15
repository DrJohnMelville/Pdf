namespace Melville.Pdf.Model.Renderers.Bitmaps;

public abstract class MaskedBitmapWriter : IComponentWriter
{
    private readonly IComponentWriter innerWriter;
    protected MaskBitmap Mask { get; }
    private readonly double widthDelta;
    private readonly double heightDelta;
    private double row;
    private double col;


    protected MaskedBitmapWriter(IComponentWriter innerWriter, MaskBitmap mask,
        int parentWidth, int parentHeight)
    {
        this.innerWriter = innerWriter;
        Mask = mask;
        widthDelta = (double)mask.Width / (parentWidth);
        heightDelta = (double)mask.Height / (parentHeight);
        col = 0;
        row = mask.Height - heightDelta;
    }

    public int ColorComponentCount => innerWriter.ColorComponentCount;
    
    public unsafe void WriteComponent(ref byte* target, int[] component, byte alpha)
    {
        innerWriter.WriteComponent(ref target, component,AlphaForByte(alpha, row, col));
        FindNextPixel();
    }

    protected abstract byte AlphaForByte(byte alpha, double atRow, double atCol);

    private void FindNextPixel()
    {
        col += widthDelta;
        if (col < Mask.Width) return;
        NextRow();
    }

    private void NextRow()
    {
        row -= heightDelta;
        col = 0;
    }
}

public class HardMaskedBitmapWriter: MaskedBitmapWriter
{
    public HardMaskedBitmapWriter(
        IComponentWriter innerWriter, MaskBitmap mask, int parentWidth, int parentHeight) : 
        base(innerWriter, mask, parentWidth, parentHeight)
    {
    }

    protected override byte AlphaForByte(byte alpha, double atRow, double atCol) =>
        (byte)(Mask.ShouldWrite((int)atRow, (int)atCol) ? alpha : 0);
}
public class SoftMaskedBitmapWriter: MaskedBitmapWriter
{
    public SoftMaskedBitmapWriter(
        IComponentWriter innerWriter, MaskBitmap mask, int parentWidth, int parentHeight) : 
        base(innerWriter, mask, parentWidth, parentHeight)
    {
    }

    protected override byte AlphaForByte(byte alpha, double atRow, double atCol) =>
        Mask.RedAt((int)atRow, (int)atCol);
}