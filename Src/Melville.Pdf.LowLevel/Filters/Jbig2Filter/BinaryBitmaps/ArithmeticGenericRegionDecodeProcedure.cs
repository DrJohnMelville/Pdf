using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;
using SixLabors.ImageSharp.ColorSpaces.Conversion;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public interface ISkipBitmap
{
    bool ShouldSkipPixel(int row, int column);
}

public class DoNotSkip: ISkipBitmap
{
    public static readonly ISkipBitmap Instance = new DoNotSkip();
    private DoNotSkip() { }
    public bool ShouldSkipPixel(int row, int column) => false;
}


public readonly struct ArithmeticGenericRegionDecodeProcedure
{
    private readonly BinaryBitmap bitmap;
    private readonly MQDecoder state;
    private readonly ArithmeticBitmapReaderContext context;
    private readonly int tgbd;
    private readonly ISkipBitmap useSkip;

    public ArithmeticGenericRegionDecodeProcedure(BinaryBitmap bitmap, MQDecoder state, ArithmeticBitmapReaderContext context, int tgbd,
        ISkipBitmap useSkip)
    {
        this.bitmap = bitmap;
        this.state = state;
        this.context = context;
        this.tgbd = tgbd;
        this.useSkip = useSkip;
    }

    public void Read(ref SequenceReader<byte> source)
    {
        bool duplicatedLastRow = false;
        var template = context.ToIncrementalTemplate();
        for (int i = 0; i < bitmap.Height; i++)
        {
            ReadRow(ref source, i, ref duplicatedLastRow, ref template);
        }
    }
    private void ReadRow(ref SequenceReader<byte> source, int row, ref bool duplicatedLastRow,
        ref IncrementalTemplate incrementalTemplate)
    {
        if (ShouldCopyPriorRow(ref source, ref duplicatedLastRow))
            bitmap.CopyRow(row - 1, row);
        else
            DecodeRow(ref source, row, ref incrementalTemplate);
    }

    private bool ShouldCopyPriorRow(ref SequenceReader<byte> source, ref bool duplicatedLastRow) => 
        IsInTgbdMode() && ShouldDuplicateThisRow(ref source, ref duplicatedLastRow);

    private bool IsInTgbdMode() => tgbd != 0;

    private bool ShouldDuplicateThisRow(ref SequenceReader<byte> source, ref bool duplicatedLastRow)
    {
        var bit = state.GetBit(ref source, ref context.GetContext(tgbd));
        return duplicatedLastRow ^= bit == 1;
    }

    private void DecodeRow(ref SequenceReader<byte> source, int row, ref IncrementalTemplate template)
    {
        template.SetToPosition(bitmap, row, 0);
        for (int j = 0; j < bitmap.Width; j++)
        {
            var bit = useSkip.ShouldSkipPixel(row, j) ? 0:
                state.GetBit(ref source, ref context.GetContext(template.context));
            #warning -- there is an opportunity to optimize writing here.
            bitmap[row, j] = bit == 1;
            // increment must follow the bitmap writting because it reads the current context byte
            template.Increment();
        }
    }
}