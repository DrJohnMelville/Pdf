namespace Melville.CCITT;

public readonly struct CcittParameters
{
    public int K { get; }
    public bool EncodedByteAlign { get; }
    public int Columns { get; }
    public int Rows { get; }
    public bool EndOfBlock { get; }
    public bool BlackIs1 { get; }
    public byte WhiteByte { get; }
    public byte BlackByte { get; }

    public CcittParameters(int k, bool encodedByteAlign, int columns, int rows, bool endOfBlock, bool blackIs1)
    {
        K = k;
        EncodedByteAlign = encodedByteAlign;
        Columns = columns;
        Rows = rows;
        EndOfBlock = endOfBlock;
        BlackIs1 = blackIs1;
        (WhiteByte, BlackByte) = BlackIs1?((byte)0,(byte)1):( (byte)1, (byte)0);
    }

    public bool[] CreateWhiteRow()
    {
        var ret = EmptyLine();
        ret.AsSpan().Fill(true);
        return ret;
    }

    public bool[] EmptyLine() => new bool[Columns];

    public bool IsWhiteValue(int value) => (value != 0) ^ BlackIs1 ;

    public bool HasReadEntireImage(int linesCompleted) =>
        Rows > 0 && linesCompleted >= Rows && !EndOfBlock;
}