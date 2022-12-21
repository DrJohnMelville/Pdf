using System;
using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.JBig2.BinaryBitmaps;

namespace Melville.JBig2.SegmentParsers.SymbolDictonaries;

internal interface IHeightClassReaderStrategy
{
    void ReadHeightClassBitmaps(ref SequenceReader<byte> source, ref SymbolParser parser, int height);
}

[StaticSingleton]
internal partial class CompositeHeightClassReader: IHeightClassReaderStrategy
{
    public void ReadHeightClassBitmaps(ref SequenceReader<byte> source, ref SymbolParser parser, int height)
    {
        var rowBitmap = ConstructCompositeBitmap(ref source, ref parser, height);
        parser.EncodedReader.ReadBitmap(ref source, rowBitmap);
    }

    private unsafe BinaryBitmap ConstructCompositeBitmap(ref SequenceReader<byte> source, ref SymbolParser parser, int height)
    {
        int* widthPtr = stackalloc int[parser.BitmapsLeftToDecode()];
        Span<int> widths = new Span<int>(widthPtr, parser.BitmapsLeftToDecode());
        int totalWidth = 0;
        int localCount = 0;
        var priorWidth = 0;
        while (parser.TryGetWidth(ref source, out var widthDelta))
        {
            widths[localCount] = (priorWidth += widthDelta);
            Debug.Assert(priorWidth > 0);
            totalWidth += priorWidth;
            localCount++;
        }
        var rowBitmap = new BinaryBitmap(height, totalWidth);
        AddBitmaps(widths[..localCount], ref parser, rowBitmap);
        return rowBitmap;
    }

    private static void AddBitmaps(
        in Span<int> widths, ref SymbolParser parser, IBinaryBitmap rowBitmap)
    {
            AddMultiBitmap(widths, ref parser, rowBitmap);
    }

    private static void AddMultiBitmap(Span<int> widths, ref SymbolParser parser, IBinaryBitmap rowBitmap)
    {
        var offset = 0;
        for (int i = 0; i < widths.Length; i++)
        {
            parser.AddBitmap(OffsetBitmapFactory.CreateHorizontalStrip(rowBitmap, offset, widths[i]));
            offset += widths[i];
        }
    }
}