using System.Buffers;
using System.Diagnostics;
using System.Linq;
using Melville.INPC;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public enum HuffmanTableType : int
{
    DC = 0x00,
    AC = 0x10
}
public partial class JpegStreamFactory
{
    private HuffmanTable GetHuffmanTable(HuffmanTableType type, int index)
    {
        var key = (int)type | index;
        foreach (var table in huffmanTables)
        {
            if (table.Key == key) return table.Value;
        }
        throw new PdfParseException("Cannot find JPEG huffman table");
    }

    [StaticSingleton]
    public partial class StartOfScanParser : IJpegBlockParser
    {
        public void ParseBlock(SequenceReader<byte> data, JpegStreamFactory factory)
        {
            var frameData = factory.frameData ??
                            throw new PdfParseException("JPEG must have SOF block before SOS block.");
            var compCount = data.ReadBigEndianUint8();
            Debug.WriteLine($"    Components: {compCount}");
            for (int i = 0; i < compCount; i++)
            {
                var cmpId = (ComponentId)data.ReadBigEndianUint8();
                var dcHuff = data.ReadBigEndianUint8();
                var acHuff = dcHuff & 0xf;
                dcHuff >>= 8;
                Debug.WriteLine($"    ComponentId: {cmpId}  acHuff: {acHuff}  dcHuff: {dcHuff}");
                frameData.UpdateHuffmanTables(cmpId,
                    factory.GetHuffmanTable(HuffmanTableType.DC, dcHuff),
                    factory.GetHuffmanTable(HuffmanTableType.AC, acHuff)
                    );
            }
        }
    }
}