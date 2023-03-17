namespace Melville.JBig2.BinaryBitmaps;

/// <summary>
/// Represents the bits of a JBIG image as a 2d array of bools
/// </summary>
public interface IJBigBitmap
{
    /// <summary>
    /// Width of the bitmap in columns
    /// </summary>
    int Width { get; }
    /// <summary>
    /// Height of the bitmap
    /// </summary>
    int Height { get; }
    /// <summary>
    /// Indexer to read or write bits
    /// </summary>
    /// <param name="row">row of the pixel</param>
    /// <param name="column">Column of the pixel</param>
    /// <returns></returns>
    bool this[int row, int column] { get; set; }
    /// <summary>
    /// The offset, in bytes, between the same column of different rows.  (May be different from width in a subbitmap)
    /// </summary>
    int Stride { get; }
    /// <summary>
    /// Get the location of a column in the first row in the underlying byte array
    /// </summary>
    /// <param name="column">Column to find</param>
    /// <returns>The underlyig array and a bitoffset into the array</returns>
    (byte[] Array, BitOffset Offset) ColumnLocation(int column);
    /// <summary>
    /// Test if the bitmap contains the given pixel
    /// </summary>
    /// <param name="row">Row of the pixel</param>
    /// <param name="col">Column of the pixel</param>
    /// <returns>True if the pixel is in the bitmap, false otherwise</returns>
    bool ContainsPixel(int row, int col);
}
internal interface IBinaryBitmap: IJBigBitmap
{
    /// <summary>
    /// Initialize a bitrow writer at a given location
    /// </summary>
    /// <param name="row">starting row</param>
    /// <param name="column">starting column</param>
    /// <returns></returns>
    BitRowWriter StartWritingAt(int row, int column);
    BitmapPointer PointerFor(int row, int col);
}

/// <summary>
/// Implements extension methods on IBinryBitmap
/// </summary>
public static class IBinaryBitmapOperations
{
    /// <summary>
    /// Size of the buffer needed to hold a binarybitmap
    /// </summary>
    /// <param name="bitmap">The binarybitmap inquired about</param>
    /// <returns>Length, in bytes of the needed bitmap.</returns>
    public static int BufferLength(this IJBigBitmap bitmap) => bitmap.Stride * bitmap.Height;
}