using System.Diagnostics;

namespace Melville.JBig2.BinaryBitmaps;

public class PageBinaryBitmap : BinaryBitmap
{
    public PageBinaryBitmap(uint height, uint width) : base((int)height, (int)width)
    {
    }

    public virtual void HandleEndOfStripe(uint yCoordinate)
    {
        Debug.Assert(yCoordinate < Height);
    }

    public virtual void DoneReading()
    {
    }
}
public class StripedBinaryBitmap : PageBinaryBitmap
{
    private readonly int maxStripe;
    private int finalizedSize = 0;
    public StripedBinaryBitmap(int maxStripe, uint width) : base((uint)maxStripe, width)
    {
        this.maxStripe = maxStripe;
    }

    public override void HandleEndOfStripe(uint yCoordinate)
    {
        var delta = yCoordinate - finalizedSize;
        finalizedSize = (int)yCoordinate;
        ResizeToHeight((int) (Height + delta));
    }

    public override void DoneReading()
    {
        ResizeToHeight(finalizedSize);
    }
}