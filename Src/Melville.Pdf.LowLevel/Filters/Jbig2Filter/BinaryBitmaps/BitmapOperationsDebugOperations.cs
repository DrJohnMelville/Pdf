using System;
using System.Linq;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public static class BitmapOperationsDebugOperations
{
    public static string BitmapString(this IBinaryBitmap src) =>
        string.Join("\r\n", Enumerable.Range(0, src.Height).Select(i=>
            string.Join("", Enumerable.Range(0,src.Width).Select(j=>src[i,j]?"B":"."))
        ));

    public static BinaryBitmap AsBinaryBitmap(this string source, int height, int width )
    {
        var ret = new BinaryBitmap(height, width);
        var count = 0;
        foreach (var character in source.Where(i=> i is 'B' or '.'))
        {
            AddBit(ret, character, count++);
        }
        return ret;
    }

    private static void AddBit(BinaryBitmap ret, char character, int position)
    {
        var (row, col) = Math.DivRem(position, ret.Width);
        ret[row, col] = character == 'B';
        // this is a utility method to help with testing, so we want the exception if too much data
    }
}