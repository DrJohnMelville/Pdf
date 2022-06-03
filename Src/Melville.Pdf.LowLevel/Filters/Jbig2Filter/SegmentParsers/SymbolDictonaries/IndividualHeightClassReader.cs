using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public class IndividualHeightClassReader: IHeightClassReaderStrategy
{
    public static readonly IndividualHeightClassReader Instance = new();
    private IndividualHeightClassReader() { }

    public void ReadHeightClassBitmaps(ref SequenceReader<byte> source, ref SymbolParser parser, int height)
    {
        var width = 0;
        while (parser.TryGetWidth(ref source, out var deltaWidth))
        {
            width += deltaWidth;
            CreateBitmap(ref source, ref parser, height, width);
        }
    }

    private void CreateBitmap(ref SequenceReader<byte> source, ref SymbolParser parser, int height, int width)
    {
        var bitmap = new BinaryBitmap(height, width);
        parser.EncodedReader.ReadBitmap(ref source, bitmap);
        parser.AddBitmap(bitmap);
    }
}