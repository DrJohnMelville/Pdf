namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class LineEncoder2D: ILineEncoder
{
    public static LineEncoder2D Instance = new LineEncoder2D();

    private LineEncoder2D() { }
    public bool TryWriteLinePrefix(ref CcittBitWriter writer, ref int a0, in LinePair lines) => true;
    
    public bool TryWriteRun(ref CcittBitWriter writer, ref int a0, in LinePair lines)
    {
        var comparison = lines.CompareLinesFrom(a0);
        if (comparison.CanPassEncode)
        {
            writer.WritePass();
            a0 = comparison.B2;
        }
        else if (comparison.CanVerticalEncode)
        {
            writer.WriteVertical(comparison.VerticalEncodingDelta);
            a0 = comparison.A1;
        }
        else
        {
            if (!writer.WriteHorizontal(lines.ImputedCurrentColor(a0),
                    comparison.FirstHorizontalDelta(a0), comparison.SecondHorizontalDelta)) return false;
            a0 = comparison.A2;
        }
        return true;
    }
}