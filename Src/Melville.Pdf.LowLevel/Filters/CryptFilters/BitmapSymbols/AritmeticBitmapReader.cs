using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public readonly struct AritmeticBitmapReader
{
    private readonly BinaryBitmap bitmap;
    private readonly MQDecoder state;
    private readonly ArithmeticBitmapReaderContext context;
    private readonly ushort tgbd;
    private readonly bool useSkip;

    public AritmeticBitmapReader(BinaryBitmap bitmap, MQDecoder state, ArithmeticBitmapReaderContext context, ushort tgbd,
        bool useSkip)
    {
        if (useSkip) throw new NotImplementedException("skipping is not implemented");
        this.bitmap = bitmap;
        this.state = state;
        this.context = context;
        this.tgbd = tgbd;
        this.useSkip = useSkip;
    }

    public void Read(ref SequenceReader<byte> source)
    {
        bool duplicatedLastRow = false;
        for (int i = 0; i < bitmap.Height; i++)
        {
            ReadRow(ref source, i, ref duplicatedLastRow);
        }

    }
    private void ReadRow(ref SequenceReader<byte> source, int row, ref bool duplicatedLastRow)
    {
        if (ShouldCopyPriorRow(ref source, ref duplicatedLastRow))
            bitmap.CopyRow(row - 1, row);
        else
            DecodeRow(ref source, row);
    }

    private bool ShouldCopyPriorRow(ref SequenceReader<byte> source, ref bool duplicatedLastRow)
    {
        return IsInTgbdMode() && ShouldDuplicateThisRow(ref source, ref duplicatedLastRow);
    }

    private bool IsInTgbdMode() => tgbd != 0;

    private bool ShouldDuplicateThisRow(ref SequenceReader<byte> source, ref bool duplicatedLastRow)
    {
        var bit = state.GetBit(ref source, ref context.GetContext(tgbd));
        return duplicatedLastRow ^= bit == 1;
    }

    private void DecodeRow(ref SequenceReader<byte> source, int row)
    {
        for (int j = 0; j < bitmap.Width; j++)
        {
            var bit = state.GetBit(ref source, ref context.ReadContext(bitmap, row, j));
            bitmap[row, j] = bit == 1;
        }
    }
}