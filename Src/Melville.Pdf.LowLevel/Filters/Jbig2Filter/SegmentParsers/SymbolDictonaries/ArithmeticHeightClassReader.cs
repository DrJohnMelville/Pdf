using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public class ArithmeticHeightClassReader: IHeightClassReaderStrategy
{
    private readonly BitmapTemplate template;

    public ArithmeticHeightClassReader(BitmapTemplate template)
    {
        this.template = template;
    }

    public void ReadHeightClassBitmaps(ref SequenceReader<byte> source, ref SymbolParser parser, int height)
    {
        throw new NotImplementedException();
    }
}