using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

public static class CrossReferenceStreamParser
{
    public static async Task<PdfDictionary> Read(ParsingFileOwner owner, long offset)
    {
        PdfObject? xRefStreamAsPdfObject ;
        var context = await owner.RentReader(offset).CA();
        xRefStreamAsPdfObject = await context.RootObjectParser.ParseAsync(context).CA();
        if (!(xRefStreamAsPdfObject is PdfStream crossRefPdfStream))
            throw new PdfParseException("Object pointed to by StartXref is not a stream");
        await ReadXrefStreamData(owner, crossRefPdfStream).CA();
        await owner.InitializeDecryption(crossRefPdfStream).CA();
        return crossRefPdfStream;
    }

    public static async Task ReadXrefStreamData(IIndirectObjectRegistry owner, PdfStream crossRefPdfStream)
    {
        await using (var decodedStream = await crossRefPdfStream.StreamContentAsync().CA())
        {
            await new ParseXRefStream(
                await crossRefPdfStream[KnownNames.W].CA(),
                await crossRefPdfStream.GetOrNullAsync(KnownNames.Index).CA(),
                decodedStream, owner
            ).Parse().CA();
        }
    }
}

public class ParseXRefStream
{
    private int nextItemNumber;
    private readonly int col0Bytes;
    private readonly int col1Bytes;
    private readonly int col2Bytes;
    private readonly int pos2;
    private readonly int rowLength;
    private readonly PipeReader source;
    private readonly IIndirectObjectRegistry registry;

    public ParseXRefStream(PdfObject W, PdfObject index, Stream sourceStream, IIndirectObjectRegistry registry)
    {
        this.registry = registry;
        source = PipeReader.Create(sourceStream);
        nextItemNumber = FindFirstItem(index);
        if (!(W is PdfArray arr &&
              arr.RawItems[0] is PdfNumber col0 &&
              arr.RawItems[1] is PdfNumber col1 &&
              arr.RawItems[2] is PdfNumber col2
            ))
            throw new PdfParseException("XRef stream does not have a valid W parameter");
        col0Bytes = (int)col0.IntValue;
        col1Bytes = (int)col1.IntValue;
        col2Bytes = (int)col2.IntValue;
        pos2 = col0Bytes + col1Bytes;
        rowLength = pos2 + col2Bytes;
    }

    private static int FindFirstItem(PdfObject index) => 
        index is PdfArray arr && arr.RawItems[0] is PdfNumber num ? (int)num.IntValue : 0;

    public async ValueTask Parse()
    {
        while (true)
        {
            var result = await source.ReadAsync().CA();
            if (DoneReading(result)) return;
            ParseFromBuffer(result.Buffer);
        }
    }

    private bool DoneReading(ReadResult result)
    {
        if (!result.IsCompleted) return false;
        if (result.Buffer.IsEmpty) return true;
        if (result.Buffer.Length >= rowLength) return false;

        throw new PdfParseException("Reference stream ends with partial reference.");
    }

    private void ParseFromBuffer(in ReadOnlySequence<byte> buffer)
    {
        if (HaveEnoughBytesToReadRow(buffer))
        {
            var lineReader = new SequenceReader<byte>(buffer);
            ParseLine(ref lineReader);
            ConsumeBytes(buffer);
        }
        else
        {
            AskForMoreBytes(buffer);
        }
    }

    private bool HaveEnoughBytesToReadRow(ReadOnlySequence<byte> buffer) => 
        buffer.Length >= rowLength;

    private void ConsumeBytes(in ReadOnlySequence<byte> buffer) => 
        source.AdvanceTo(buffer.GetPosition(rowLength));

    private void AskForMoreBytes(in ReadOnlySequence<byte> buffer) => 
        source.AdvanceTo(buffer.Start, buffer.End);

    private void ParseLine(ref SequenceReader<byte> reader)
    {
        var c0 = GetInt(ref reader, col0Bytes);
        var c1 = GetInt(ref reader, col1Bytes);
        var c2 = GetInt(ref reader, col2Bytes);
        ProcessLine(c0, c1, c2);
    }

    private long GetInt(ref SequenceReader<byte> reader, int bytesToRead)
    {
        var ret = 0;
        for (int j = 0; j < bytesToRead; j++)
        {
            ret <<= 8;
            reader.TryRead(out byte datum); // must succeed because we checked length above
            ret |= datum;
        }
        return ret;
    }

    private void ProcessLine(long c0, long c1, long c2)
    {
        switch (c0)
        {
            case 0: 
                registry.RegisterDeletedBlock(nextItemNumber++, (int)c2, (int)c1);
                break;
            case 1:
                registry.RegisterIndirectBlock(nextItemNumber++, c2, c1);
                break;
            case 2:
                registry.RegisterObjectStreamBlock(nextItemNumber++, c1, c2);
                break;
            default:
                registry.RegistedNullObject(nextItemNumber++, (int)c2, (int)c1);
                break;
        }
    }
}