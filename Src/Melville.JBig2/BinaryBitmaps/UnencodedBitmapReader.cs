using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.IO;

namespace Melville.JBig2.BinaryBitmaps;

internal static class UnencodedBitmapReader
{
    public static void ReadUnencodedBitmap(this BinaryBitmap bitmap,  ref SpanParsingBuffer reader)
    {
        var (array, _) = bitmap.ColumnLocation(0);

        if (reader.CurrentLength < array.Length)
            throw new InvalidDataException("Not enough bytes in unencoded bitmap");
        reader.Peek(array.Length).CopyTo(array);
        reader.Advance(array.Length);
        
    }
}