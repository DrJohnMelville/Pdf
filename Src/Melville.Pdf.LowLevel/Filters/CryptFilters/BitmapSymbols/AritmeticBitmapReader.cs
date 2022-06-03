
using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public static class AritmeticBitmapReader
{
    public static void ReadArithmeticEncodedBitmap(
        this BinaryBitmap bitmap, ref SequenceReader<byte> source,
        MQDecoder state, ArithmeticBitmapReaderContext context, bool tgbd, bool useSkip)
    {
        if (tgbd) throw new NotImplementedException("Typical prediction for generic direct coding is not implemented");
        if (useSkip) throw new NotImplementedException("skipping is not implemented");
        for (int i = 0; i < bitmap.Height; i++)
        {
            for (int j = 0; j < bitmap.Width; j++)
            {
                var bit = state.GetBit(ref source, ref context.ReadContext(bitmap, i, j));
                bitmap[i, j] = bit == 1;
            }
        }
    }
}

public readonly struct ArithmeticBitmapReaderContext
{
    private readonly BitmapTemplate template;
    private readonly ContextStateDict dictionary;

    public ArithmeticBitmapReaderContext(BitmapTemplate template) : this()
    {
        this.template = template;
        dictionary = new ContextStateDict(this.template.BitsRequired());
    }

    public ref ContextEntry ReadContext(BinaryBitmap bitmap, int row, int col) =>
        ref dictionary.EntryForContext(template.ReadContext(bitmap, row, col));
}