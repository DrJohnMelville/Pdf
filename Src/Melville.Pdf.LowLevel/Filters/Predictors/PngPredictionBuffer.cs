namespace Melville.Pdf.LowLevel.Filters.Predictors;

internal class PngPredictionBuffer
{
    private byte[] priorLine;
    private byte[] currentLine;
    private readonly int bytesPerPixel;
    private int column;

    public PngPredictionBuffer(int colors, int bitsPerColor, int columns)
    {
        bytesPerPixel = ScanLineLengthComputer.BitsToBytesRoundUp(colors * bitsPerColor);
        column = ScanLineLengthComputer.ComputeGroupsPerRow(colors, bitsPerColor, columns,8);
        currentLine = new byte[column];
        priorLine = new byte[column];
    }
        
    public byte Left => ByteFromPriorPixel(currentLine);
    public byte UpLeft => ByteFromPriorPixel(priorLine);
    public byte Up => priorLine[column];
    public void RecordColumnValue(byte b) => currentLine[column] = b;
    public bool AtEndOfRow() => column >= currentLine.Length;
    public void AdvanceToNextColumn() => column++;
    public void AdvanceToNextRow()
    {
        (currentLine, priorLine) = (priorLine, currentLine);
        column = 0;
    }
        
    private byte ByteFromPriorPixel(byte[] bytes)
    {
        var index = column - bytesPerPixel;
        return index < 0 ? (byte)0 : bytes[index];
    }
}