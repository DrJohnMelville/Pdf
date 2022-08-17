using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegStreamFactory
{
    private const int JpegTagSize = 2;
    private readonly PipeReader source;
    private JpegFrameData? frameData;
    private readonly QuantizationTable?[] quantizationTables = new QuantizationTable?[16];

    public JpegStreamFactory(Stream sourceStream)
    {
        source = PipeReader.Create(sourceStream);
    }

    public async ValueTask<ReadJpegStream> Construct()
    {
        while (true)
        {
            var jpegBlockType = (JpegBlockType)await ReadU16().CA();
            switch (jpegBlockType)
            {
                case JpegBlockType.StartOfImage:
                    Debug.WriteLine("Start of Image");
                    break;
                case JpegBlockType.StartOfScan:
                    Debug.WriteLine("StartOfScan");
                    return new ReadJpegStream(source, TryGetFrameData());
                case JpegBlockType.EndOfImage:
                    Debug.Assert(false, "Should not have end of image before start of SCAN");
                    break;
                case var tag:
                    Debug.WriteLine($"Other Tag 0x{tag:X} -{tag}");
                    await ParseBlock(PickParser(tag)).CA();
                    break;
            }
        }
    }

    private JpegFrameData TryGetFrameData() =>
        frameData?? throw new PdfParseException("Jpeg Mission Start Of Frame Tag");

    private IJpegBlockParser PickParser(JpegBlockType type) => type switch
    {
      JpegBlockType.ApplicationDefaultHeader => VerifyApp0Segment.Instance,
      JpegBlockType.StartOfFrame => StartOfFrameParser.Instance,
      JpegBlockType.QuantizationTable => QuantizationTableParser.Instance,
      JpegBlockType.DefineHuffmanTable => HuffmanTableParser.Instance,
      _ => IgnoreBlockParser.Instance
    } ;

    private async ValueTask<int> ReadU16()
    {
        var result = await source.ReadAtLeastAsync(JpegTagSize).CA();
        var ret = ReadU16(result.Buffer);
        source.AdvanceTo(result.Buffer.GetPosition(JpegTagSize));
        return ret;
    }

    private int ReadU16(ReadOnlySequence<byte> resultBuffer)
    {
        var reader = new SequenceReader<byte>(resultBuffer);
        return reader.ReadBigEndianUint16();
    }

    private async ValueTask ParseBlock(IJpegBlockParser parser)
    {
        int remainingLength = (await ReadU16().CA())-JpegTagSize;
        var result = await source.ReadAtLeastAsync(remainingLength).CA();
        var block = result.Buffer.Slice(0, remainingLength);
        parser.ParseBlock(new SequenceReader<byte>(block),this);
        source.AdvanceTo(block.End);
    }
}
