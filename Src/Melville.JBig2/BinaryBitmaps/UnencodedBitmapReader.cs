using System.Buffers;

namespace Melville.JBig2.BinaryBitmaps;

public static class UnencodedBitmapReader
{
    public static void ReadUnencodedBitmap(this BinaryBitmap bitmap,  ref SequenceReader<byte> reader)
    {
        var (array, _) = bitmap.ColumnLocation(0);
        if (!reader.TryCopyTo(array.AsSpan()))
            throw new InvalidDataException("Not enough bytes in unencoded bitmap");
        reader.Advance(array.Length);
        
    }
}