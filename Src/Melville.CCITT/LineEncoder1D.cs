namespace Melville.CCITT;

internal class LineEncoder1D : ILineEncoder
{
    public static readonly ILineEncoder Instance = new LineEncoder1D();
    private LineEncoder1D() { }
    
    public bool TryWriteLinePrefix(ref CcittBitWriter writer, ref int a0, in LinePair lines)
    {
        if (!EncodingImaginaryFirstPixel(a0)) return true;
        if (! (lines.CurrentLine[0] || WriteEmptyWhiteRun(ref writer))) return false;
        a0 = 0;
        return true;
    }
    private bool EncodingImaginaryFirstPixel(int a0) => a0 <0;
    private bool WriteEmptyWhiteRun(ref CcittBitWriter writer) => writer.WriteHorizontal(true, 0);

    public bool TryWriteRun(ref CcittBitWriter writer, ref int a0, in LinePair lines)
    {
        var a1 = lines.StartOfNextRun(a0);
        if (!writer.WriteHorizontal(lines.CurrentLine[a0], a1 - a0)) return false;
        a0 = a1;
        return true;
    }
}