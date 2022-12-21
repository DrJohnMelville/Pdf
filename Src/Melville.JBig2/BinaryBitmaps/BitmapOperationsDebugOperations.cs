using System;
using System.Collections.Generic;
using System.Linq;

namespace Melville.JBig2.BinaryBitmaps;

internal static class BitmapOperationsDebugOperations
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

public static class StringJoiner
{
    public static string JoinLines(string a, string b, string divider)
    {
        var listA = SplitLines(a);
        var listB = SplitLines(b);
        return (listA.Length - listB.Length) switch
        {
            < 0 => JoinLines(Pad(listA, StringOfLength(listA[0].Length)), listB, divider),
            0 => JoinLines(listA, listB, divider),
            > 0 => JoinLines(listA, Pad(listB, StringOfLength(listB[0].Length)), divider)
        };
    }

    private static string JoinLines(IEnumerable<string> listA, IEnumerable<string> listB, string divider) => 
        string.Join("\r\n", listA.Zip(listB, (a, b) => $"{a}{divider}{b}"));

    private static IEnumerable<string> Pad(string[] listA, string extra)
    {
        foreach (var item in listA) yield return item;
        while (true) yield return extra;
    }

    private static string StringOfLength(int length)
    {
        return string.Join("", Enumerable.Repeat(" ", length));
    }

    private static string[] SplitLines(string a)
    {
        return a.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
