using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public interface IHeightClassReaderStrategy
{
    void ReadHeightClassBitmaps(ref BitSource source, ref SymbolParser parser, int height);
}

public class CompositeHeightClassReaderStrategy: IHeightClassReaderStrategy
{
    public static CompositeHeightClassReaderStrategy Instance = new();

    private CompositeHeightClassReaderStrategy() { }

    public void ReadHeightClassBitmaps(ref BitSource source, ref SymbolParser parser, int height)
    {
        var rowBitmap = ConstructCompositeBitmap(ref source, ref parser, height);
        var bitmapLength = parser.SizeReader.GetInteger(ref source);
        var reader = source.Source;
        if (bitmapLength == 0)
            rowBitmap.ReadUnencodedBitmap(ref reader);
        else
            rowBitmap.ReadMmrEncodedBitmap(ref reader, false);
        parser.AdvancePast(reader);
    }

    private unsafe BinaryBitmap ConstructCompositeBitmap(ref BitSource source, ref SymbolParser parser, int height)
    {
        int* widthPtr = stackalloc int[parser.BitmapsLeftToDecode()];
        Span<int> widths = new Span<int>(widthPtr, parser.BitmapsLeftToDecode());
        int totalWidth = 0;
        int localCount = 0;
        var priorWidth = 0;
        while (TryGetWidth(ref source, ref parser, out var widthDelta))
        {
            widths[localCount] = (priorWidth += widthDelta);
            totalWidth += priorWidth;
            localCount++;
        }
        var rowBitmap = new BinaryBitmap(height, totalWidth);
        AddBitmaps(widths[..localCount], ref parser, rowBitmap);
        return rowBitmap;
    }
    
    private bool TryGetWidth(ref BitSource source, ref SymbolParser parser, out int value) => 
        !parser.WidthReader.IsOutOfBand(value = parser.WidthReader.GetInteger(ref source));

    private static void AddBitmaps(
        in Span<int> widths, ref SymbolParser parser, IBinaryBitmap rowBitmap)
    {
        if (widths.Length == 1)
            parser.AddBitmap(rowBitmap);
        else
            AddMultiBitmap(widths, ref parser, rowBitmap);
    }

    private static void AddMultiBitmap(Span<int> widths, ref SymbolParser parser, IBinaryBitmap rowBitmap)
    {
        var offset = 0;
        for (int i = 0; i < widths.Length; i++)
        {
            parser.AddBitmap(new HorizontalStripBitmap(rowBitmap, offset, widths[i]));
            offset += widths[i];
        }
    }
}