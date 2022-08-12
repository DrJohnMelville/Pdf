using System.Buffers;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public interface IIndividualBitmapReader
{
    void ReadBitmap(ref SequenceReader<byte> source, ref SymbolParser reader, BinaryBitmap bitmap);
}

[StaticSingleton]
public partial class IndividualHeightClassReader: IHeightClassReaderStrategy
{
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
        parser.IndividualBitmapReader.ReadBitmap(ref source, ref parser, bitmap);
        parser.AddBitmap(bitmap);
    }
}

[StaticSingleton]
public sealed partial class UnrefinedBitmapReader : IIndividualBitmapReader
{
    public void ReadBitmap(ref SequenceReader<byte> source, ref SymbolParser reader, BinaryBitmap bitmap)
    {
        reader.EncodedReader.ReadBitmap(ref source, bitmap);
    }
}