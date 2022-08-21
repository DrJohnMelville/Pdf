namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public static class ZizZagPositions
{
    public static readonly int[] ZigZagToMatrix  = 
    {
        Off(0, 0),
        Off(0, 1), Off(1, 0),
        Off(2, 0), Off(1, 1), Off(0, 2),
        Off(0, 3), Off(1, 2), Off(2, 1), Off(3, 0),
        Off(4, 0), Off(3, 1), Off(2, 2), Off(1, 3), Off(0, 4),
        Off(0, 5), Off(1, 4), Off(2, 3), Off(3, 2), Off(4, 1), Off(5, 0),
        Off(6, 0), Off(5, 1), Off(4, 2), Off(3, 3), Off(2, 4), Off(1, 5), Off(0, 6),
        Off(0, 7), Off(1, 6), Off(2, 5), Off(3, 4), Off(4, 3), Off(5, 2), Off(6, 1), Off(7, 0),
        Off(7, 1), Off(6, 2), Off(5, 3), Off(4, 4), Off(3, 5), Off(2, 6), Off(1, 7),
        Off(2, 7), Off(3, 6), Off(4, 5), Off(5, 4), Off(6, 3), Off(7, 2),
        Off(7, 3), Off(6, 4), Off(5, 5), Off(4, 6), Off(3, 7),
        Off(4, 7), Off(5, 6), Off(6, 5), Off(7, 4),
        Off(7, 5), Off(6, 6), Off(5, 7),
        Off(6, 7), Off(7, 6),
        Off(7, 7)
    };
    private static int Off(int row, int column) => row * 8 + column;

}