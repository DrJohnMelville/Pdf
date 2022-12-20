namespace Melville.CCITT;

/// <summary>
/// Parameters from a PDF file which set various parameters of a CCITT decryption filter
/// </summary>
public readonly struct CcittParameters
{
    /// <summary>
    /// Encoding scheme See Spec 7.4.6 Table 11
    /// </summary>
    public int K { get; }
    /// <summary>
    /// If true, the bit sream  has extra 0 bits on the end of each line so the lines are byte aligned
    /// </summary>
    public bool EncodedByteAlign { get; }
    /// <summary>
    /// Number of columns in the bitmap.
    /// </summary>
    public int Columns { get; }
    /// <summary>
    /// Number of rows in the bitmap
    /// </summary>
    public int Rows { get; }
    /// <summary>
    /// An end of block sequence is expected at the end of the data.
    /// </summary>
    public bool EndOfBlock { get; }
    /// <summary>
    /// Indicatessss the black pixels are or are not encoded with a 1 bit.
    /// </summary>
    public bool BlackIs1 { get; }
    /// <summary>
    /// The bit (0 or 1) that encodes black pixels
    /// </summary>
    internal byte WhiteByte { get; }
    /// <summary>
    /// The bit (0 or 1) that encodes white pixels.
    /// </summary>
    internal byte BlackByte { get; }

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

    internal bool[] CreateWhiteRow()
    {
        var ret = EmptyLine();
        ret.AsSpan().Fill(true);
        return ret;
    }

    internal bool[] EmptyLine() => new bool[Columns];

    internal bool IsWhiteValue(int value) => (value != 0) ^ BlackIs1 ;

    internal bool HasReadEntireImage(int linesCompleted) =>
        Rows > 0 && linesCompleted >= Rows && !EndOfBlock;
}