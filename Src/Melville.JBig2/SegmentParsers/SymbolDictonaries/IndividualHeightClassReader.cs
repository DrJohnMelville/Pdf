using System.Buffers;
using Melville.INPC;
using Melville.JBig2.BinaryBitmaps;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.SegmentParsers.SymbolDictonaries;

internal interface IIndividualBitmapReader
{
    void ReadBitmap(ref SpanParsingBuffer source, ref SymbolParser reader, BinaryBitmap bitmap);
}

[StaticSingleton]
internal partial class IndividualHeightClassReader: IHeightClassReaderStrategy
{
    public void ReadHeightClassBitmaps(ref SpanParsingBuffer source, ref SymbolParser parser, int height)
    {
        var width = 0;
        while (parser.TryGetWidth(ref source, out var deltaWidth))
        {
            width += deltaWidth;
            CreateBitmap(ref source, ref parser, height, width);
        }
    }

    private void CreateBitmap(ref SpanParsingBuffer source, ref SymbolParser parser, int height, int width)
    {
        var bitmap = new BinaryBitmap(height, width);
        parser.IndividualBitmapReader.ReadBitmap(ref source, ref parser, bitmap);
        parser.AddBitmap(bitmap);
    }
}

[StaticSingleton]
internal sealed partial class UnrefinedBitmapReader : IIndividualBitmapReader
{
    public void ReadBitmap(ref SpanParsingBuffer source, ref SymbolParser reader, BinaryBitmap bitmap)
    {
        reader.EncodedReader.ReadBitmap(ref source, bitmap);
    }
}